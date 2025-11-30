// Tests/Application/Administrator/Update/HospitalizePatientUseCaseTests.cs

using FluentAssertions;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Update
{
    public class HospitalizePatientUseCaseTests
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
        public async Task Deve_Hospitalizar_Paciente_Com_Sucesso()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);
            var healthUnitRepository = new HealthUnitRepository(context);
            var hospitalizationRepository = new HospitalizationRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);
            Guid bedId = await CreateHealthUnitWithSingleBedAsync(healthUnitRepository, BedStatus.Available);

            var request = HospitalizationGenerator.GenerateHospitalizeRequest(
                patientId: patientId,
                bedId: bedId
            );

            var useCase = new HospitalizePatientUseCase(patientRepository, bedRepository, hospitalizationRepository);

            // Act
            var response = await useCase.Handle(request);

            // Assert - response
            response.Should().NotBeNull();
            response.PatientId.Should().Be(patientId);
            response.BedId.Should().Be(bedId);
            response.HospitalizationId.Should().NotBe(Guid.Empty);
            response.AdmissionDate.Should().NotBe(default);
            response.Reason.Should().Be(request.Reason);

            // Assert - paciente e internação
            var persistedPatient = await patientRepository.GetByIdAsync(patientId);
            persistedPatient.Should().NotBeNull();
            persistedPatient!.Hospitalizations.Should().HaveCount(1);

            var hospitalization = persistedPatient.Hospitalizations.Single();
            hospitalization.Id.Should().Be(response.HospitalizationId);
            hospitalization.Bed.Id.Should().Be(bedId);
            hospitalization.Reason.Should().Be(request.Reason);
            hospitalization.Status.Should().Be(HospitalizationStatus.Admitted);
            hospitalization.DischargeDate.Should().BeNull();

            // Assert - cama ocupada
            var persistedBed = await bedRepository.GetByIdAsync(bedId);
            persistedBed.Should().NotBeNull();
            persistedBed!.Status.Should().Be(BedStatus.Occupied);
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Paciente_Nao_Existir()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);
            var hospitalizationRepository = new HospitalizationRepository(context);
            var healthUnitRepository = new HealthUnitRepository(context);

            // Criamos um leito válido só para garantir cenário realista
            Guid bedId = await CreateHealthUnitWithSingleBedAsync(healthUnitRepository, BedStatus.Available);

            var request = HospitalizationGenerator.GenerateHospitalizeRequest(
                patientId: Guid.NewGuid(), // paciente inexistente
                bedId: bedId
            );

            var useCase = new HospitalizePatientUseCase(patientRepository, bedRepository, hospitalizationRepository);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Nenhum paciente foi encontrado com o ID informado.");
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Cama_Nao_Existir()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);
            var hospitalizationRepository = new HospitalizationRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);

            var request = HospitalizationGenerator.GenerateHospitalizeRequest(
                patientId: patientId,
                bedId: Guid.NewGuid() // cama inexistente
            );

            var useCase = new HospitalizePatientUseCase(patientRepository, bedRepository, hospitalizationRepository);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Nenhuma cama foi encontrada com o ID informado.");
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Cama_Nao_Estiver_Disponivel()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);
            var hospitalizationRepository = new HospitalizationRepository(context);
            var healthUnitRepository = new HealthUnitRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);
            Guid bedId = await CreateHealthUnitWithSingleBedAsync(healthUnitRepository, BedStatus.Occupied);

            var request = HospitalizationGenerator.GenerateHospitalizeRequest(
                patientId: patientId,
                bedId: bedId
            );

            var useCase = new HospitalizePatientUseCase(patientRepository, bedRepository, hospitalizationRepository);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A cama informada não está disponível para internação.");

            var persistedBed = await bedRepository.GetByIdAsync(bedId);
            persistedBed.Should().NotBeNull();
            persistedBed!.Status.Should().Be(BedStatus.Occupied);

            var persistedPatient = await patientRepository.GetByIdAsync(patientId);
            persistedPatient.Should().NotBeNull();
            persistedPatient!.Hospitalizations.Should().BeEmpty();
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Paciente_Ja_Possuir_Internacao_Ativa()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var patientRepository = new PatientRepository(context);
            var bedRepository = new BedRepository(context);
            var hospitalizationRepository = new HospitalizationRepository(context);
            var healthUnitRepository = new HealthUnitRepository(context);

            Guid patientId = await CreatePatientAsync(patientRepository);

            // Cria uma unidade com dois leitos:
            // - um já ocupado com internação ativa
            // - outro disponível que será usado na tentativa de nova internação
            var healthUnitRequest = HealthUnitGenerator.GenerateHealthUnit();
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepository);
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitRequest);

            var healthUnit = await healthUnitRepository.GetByIdAsync(healthUnitResult.HealthUnitId);
            healthUnit.Should().NotBeNull();

            var activeBed = new Bed
            {
                BedNumber = "BED-ACTIVE",
                Type = BedType.General,
                Status = BedStatus.Occupied
            };

            var candidateBed = new Bed
            {
                BedNumber = "BED-CANDIDATE",
                Type = BedType.General,
                Status = BedStatus.Available
            };

            healthUnit!.Beds.Add(activeBed);
            healthUnit.Beds.Add(candidateBed);
            await healthUnitRepository.UpdateAsync(healthUnit);

            // Recupera o paciente para associar a internação ativa
            var patient = await patientRepository.GetByIdAsync(patientId);
            patient.Should().NotBeNull();

            var activeHospitalization = new Hospitalization
            {
                AdmissionDate = DateTimeOffset.UtcNow.AddHours(-2),
                DischargeDate = null, // ainda internado
                Reason = "Internação prévia ainda ativa.",
                Status = HospitalizationStatus.Admitted,
                Patient = patient!,
                Bed = activeBed
            };

            patient!.Hospitalizations.Add(activeHospitalization);
            await patientRepository.UpdateAsync(patient);

            var request = HospitalizationGenerator.GenerateHospitalizeRequest(
                patientId: patientId,
                bedId: candidateBed.Id
            );

            var useCase = new HospitalizePatientUseCase(patientRepository, bedRepository, hospitalizationRepository);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("O paciente já possui uma internação ativa.");

            // Garante que não foi criada nova internação
            var persistedPatient = await patientRepository.GetByIdAsync(patientId);
            persistedPatient.Should().NotBeNull();
            persistedPatient!.Hospitalizations.Should().HaveCount(1);

            // Garante que a cama candidata permanece disponível
            var persistedCandidateBed = await bedRepository.GetByIdAsync(candidateBed.Id);
            persistedCandidateBed.Should().NotBeNull();
            persistedCandidateBed!.Status.Should().Be(BedStatus.Available);
        }
    }
}
