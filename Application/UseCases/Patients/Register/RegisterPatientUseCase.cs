// Application/UseCases/Patients/Register/RegisterPatientUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Mappers;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.Patients.Register
{
    /// <summary>
    /// Caso de uso responsável por orquestrar o fluxo de registro de um novo paciente.
    /// </summary>
    /// <remarks>
    /// Este caso de uso aplica as regras de negócio necessárias para criação de pacientes,
    /// incluindo verificação de CPF duplicado e conversão de DTOs em entidades e value objects
    /// de domínio. A persistência efetiva é delegada ao repositório de pacientes.
    /// </remarks>
    public sealed class RegisterPatientUseCase
    {
        private readonly IPatientRepository _patientRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de registro de paciente.
        /// </summary>
        /// <param name="patientRepository">Repositório de pacientes utilizado para acesso a dados.</param>
        public RegisterPatientUseCase(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        /// <summary>
        /// Executa o fluxo de registro de um novo paciente com base nos dados fornecidos.
        /// </summary>
        /// <param name="request">Dados de entrada necessários para registrar o paciente.</param>
        /// <param name="cancellationToken">Token de cancelamento opcional.</param>
        /// <returns>Um <see cref="RegisterPatientResponse"/> contendo o identificador do paciente criado.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando já existe um paciente cadastrado com o CPF informado.
        /// </exception>
        public async Task<RegisterPatientResponse> Handle(RegisterPatientRequest request)
        {
            // Regra de negócio: paciente deve ser maior de idade
            if (!IsAdult(request.BirthDate))
            {
                throw new InvalidOperationException("O paciente deve ser maior de idade.");
            }

            // Conversão dos dados do DTO para Value Objects de domínio
            var email = new Email(request.Email);
            var phone = new Phone(request.Phone);
            var cpf = new Cpf(request.Cpf);
            var password = Password.Create(request.Password);

            // Regra de negócio: não permitir CPF duplicado
            bool cpfAlreadyExists = await _patientRepository.ExistsByCpfAsync(cpf);

            if (cpfAlreadyExists)
            {
                throw new InvalidOperationException("Já existe um paciente cadastrado com o CPF informado.");
            }

            // Regra de negócio: não permitir Email duplicado
            bool emailAlreadyExists = await _patientRepository.ExistsByEmailAsync(email);

            if (emailAlreadyExists)
            {
                throw new InvalidOperationException("Já existe um paciente cadastrado com o Email informado.");
            }


            Address address = AddressMapper.ToDomain(request.Address);

            // Construção da entidade de domínio Patient
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = email,
                Password = password,
                Phone = phone,
                Status = UserStatus.Active,
                Cpf = cpf,
                BirthDate = request.BirthDate,
                Sex = request.Sex,
                Address = address,
                EmergencyContactName = request.EmergencyContactName
            };

            // Garante que veio ao menos um consent
            if (request.Consents is null || !request.Consents.Any())
            {
                throw new InvalidOperationException("É obrigatório informar ao menos um consentimento.");
            }

            // Associação dos consentimentos fornecidos ao paciente
            foreach (var dto in request.Consents)
            {
                var consent = ConsentMapper.ToDomain(dto);
                patient.Consents.Add(consent);
            }

            // Usa o método do model para validar consentimento ativo
            var treatmentConsent = patient.GetActiveConsent(ConsentScope.Treatment);

            if (treatmentConsent is null)
            {
                throw new InvalidOperationException(
                    "O paciente não possui um consentimento de tratamento ativo."
                );
            }

            // Criação do prontuário novo para o paciente
            var medicalRecord = CreateEmptyMedicalRecordFor(patient);
            patient.MedicalRecord = medicalRecord;

            // Persistência
            await _patientRepository.AddAsync(patient);

            return new RegisterPatientResponse(patient.Id);
        }

        /// <summary>
        /// Determina se a data de nascimento corresponde a um adulto (18+).
        /// </summary>
        private static bool IsAdult(DateTimeOffset birthDate)
        {
            var today = DateTimeOffset.UtcNow.Date;
            var birth = birthDate.Date;

            int age = today.Year - birth.Year;

            // Se ainda não fez aniversário neste ano, decrementa
            if (birth > today.AddYears(-age))
            {
                age--;
            }

            return age >= 18;
        }
        /// <summary>
        /// Cria um prontuário "virgem" para o paciente, gerando um MRN válido.
        /// </summary>
        private static MedicalRecord CreateEmptyMedicalRecordFor(Patient patient)
        {
            var mrnString = GenerateMedicalRecordNumber();
            var mrn = new MedicalRecordNumber(mrnString);

            return new MedicalRecord
            {
                Id = Guid.NewGuid(),
                Number = mrn,
                CreatedAt = DateTimeOffset.UtcNow,
                Patient = patient
            };
        }

        /// <summary>
        /// Gera um número de prontuário válido segundo as regras de <see cref="MedicalRecordNumber"/>.
        /// </summary>
        /// <remarks>
        /// Usa um GUID como base, garantindo apenas A–F e 0–9, tamanho dentro da faixa
        /// e rejeitando o caso extremo de todos os caracteres iguais.
        /// </remarks>
        private static string GenerateMedicalRecordNumber()
        {
            while (true)
            {
                // 32 chars A–F0–9
                string raw = Guid.NewGuid().ToString("N").ToUpperInvariant();

                // recorta para algo entre 6 e 20, ex.: 12
                string candidate = raw[..12];

                // Proteção extra: evita todos iguais (regra do VO)
                if (!candidate.All(c => c == candidate[0]))
                {
                    return candidate;
                }
            }
        }
    }
}
