// Tests/Application/Administrator/Update/DischargePatientUseCaseTests.cs

using FluentAssertions;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Update
{
    public class DischargePatientUseCaseTests
    {
        /// <summary>
        /// Cria e persiste um paciente válido com consentimentos.
        /// </summary>
        private static async Task<Guid> CreatePatientAsync(PatientRepository patientRepository)
        {
            RegisterPatientRequest request = PatientGenerator.GeneratePatient();
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            request.Consents.Add(treatmentConsent);
            request.Consents.Add(researchConsent);
            request.Consents.Add(notificationConsent);

            var useCase = new RegisterPatientUseCase(patientRepository);
            var result = await useCase.Handle(request);

            return result.PatientId;
        }

        /// <summary>
        /// Cria uma unidade de saúde e adiciona um único leito com o status informado.
        /// Retorna o identificador do leito criado.
        /// </summary>
        private static async Task<Guid> CreateHealthUnitWithSingleBedAsync(
            HealthUnitRepository healthUnitRepository,
            BedStatus bedStatus)
        {
            var request = HealthUnitGenerator.GenerateHealthUnit();
            var useCase = new RegisterHealthUnitUseCase(healthUnitRepository);
            var result = await useCase.Handle(request);

            var healthUnit = await healthUnitRepository.GetByIdAsync(result.HealthUnitId);
            healthUnit.Should().NotBeNull();

            var bed = new Bed
            {
                BedNumber = "BED-001",
                Type = BedType.General,
                Status = bedStatus
            };

            healthUnit!.Beds.Add(bed);
            await healthUnitRepository.UpdateAsync(healthUnit);

            return bed.Id;
        }

        [Fact]
        public async Task Deve_Dar_Alta_Paciente_Com_Sucesso()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);
            var healthUnitRepository = new HealthUnitRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);
            Guid bedId = await CreateHealthUnitWithSingleBedAsync(healthUnitRepository, BedStatus.Occupied);

            var bed = await bedRepository.GetByIdAsync(bedId);
            bed.Should().NotBeNull();

            var patient = await patientRepository.GetByIdAsync(patientId);
            patient.Should().NotBeNull();

            var admissionDate = DateTimeOffset.UtcNow.AddHours(-4);

            var activeHospitalization = new Hospitalization
            {
                AdmissionDate = admissionDate,
                DischargeDate = null,
                Reason = "Internação de teste para alta.",
                Status = HospitalizationStatus.Admitted,
                Patient = patient!,
                Bed = bed!
            };

            patient!.Hospitalizations.Add(activeHospitalization);
            await patientRepository.UpdateAsync(patient);

            var useCase = new DischargePatientUseCase(patientRepository, bedRepository);

            // Act
            await useCase.Handle(patientId);

            // Assert - paciente e internação
            var persistedPatient = await patientRepository.GetByIdAsync(patientId);
            persistedPatient.Should().NotBeNull();
            persistedPatient!.Hospitalizations.Should().HaveCount(1);

            var hospitalization = persistedPatient.Hospitalizations.Single();
            hospitalization.Status.Should().Be(HospitalizationStatus.Discharged);
            hospitalization.DischargeDate.Should().NotBeNull();
            hospitalization.DischargeDate.Should().BeAfter(admissionDate);

            // Assert - cama liberada
            var persistedBed = await bedRepository.GetByIdAsync(bedId);
            persistedBed.Should().NotBeNull();
            persistedBed!.Status.Should().Be(BedStatus.Available);
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Paciente_Nao_Existir()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);

            var useCase = new DischargePatientUseCase(patientRepository, bedRepository);
            Guid nonexistentPatientId = Guid.NewGuid();

            // Act
            Func<Task> act = () => useCase.Handle(nonexistentPatientId);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Nenhum paciente foi encontrado com o ID informado.");
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Paciente_Nao_Possuir_Internacao_Ativa()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);

            var useCase = new DischargePatientUseCase(patientRepository, bedRepository);

            // Act
            Func<Task> act = () => useCase.Handle(patientId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("O paciente não possui uma internação ativa.");
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Internacao_Ativa_Nao_Possuir_Cama_Associada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);

            var patient = await patientRepository.GetByIdAsync(patientId);
            patient.Should().NotBeNull();

            var activeHospitalization = new Hospitalization
            {
                AdmissionDate = DateTimeOffset.UtcNow.AddHours(-1),
                DischargeDate = null,
                Reason = "Internação sem cama para teste.",
                Status = HospitalizationStatus.Admitted,
                Patient = patient!,
                Bed = null!
            };

            patient!.Hospitalizations.Add(activeHospitalization);
            await patientRepository.UpdateAsync(patient);

            var useCase = new DischargePatientUseCase(patientRepository, bedRepository);

            // Act
            Func<Task> act = () => useCase.Handle(patientId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A internação ativa não está associada a nenhuma cama.");
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Cama_Associada_Nao_Estiver_Ocupada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);
            var healthUnitRepository = new HealthUnitRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);
            Guid bedId = await CreateHealthUnitWithSingleBedAsync(healthUnitRepository, BedStatus.Available);

            var bed = await bedRepository.GetByIdAsync(bedId);
            bed.Should().NotBeNull();

            var patient = await patientRepository.GetByIdAsync(patientId);
            patient.Should().NotBeNull();

            var activeHospitalization = new Hospitalization
            {
                AdmissionDate = DateTimeOffset.UtcNow.AddHours(-2),
                DischargeDate = null,
                Reason = "Internação com cama em estado inconsistente.",
                Status = HospitalizationStatus.Admitted,
                Patient = patient!,
                Bed = bed! // cama não está Occupied
            };

            patient!.Hospitalizations.Add(activeHospitalization);
            await patientRepository.UpdateAsync(patient);

            var useCase = new DischargePatientUseCase(patientRepository, bedRepository);

            // Act
            Func<Task> act = () => useCase.Handle(patientId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A cama associada à internação ativa não está ocupada.");

            // Garante que nada foi alterado indevidamente
            var persistedBed = await bedRepository.GetByIdAsync(bedId);
            persistedBed.Should().NotBeNull();
            persistedBed!.Status.Should().Be(BedStatus.Available);

            var persistedPatient = await patientRepository.GetByIdAsync(patientId);
            persistedPatient.Should().NotBeNull();
            persistedPatient!.Hospitalizations.Should().HaveCount(1);
            persistedPatient.Hospitalizations.Single().DischargeDate.Should().BeNull();
            persistedPatient.Hospitalizations.Single().Status.Should().Be(HospitalizationStatus.Admitted);
        }
    }
}
