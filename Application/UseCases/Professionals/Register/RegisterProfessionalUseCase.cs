// Application/UseCases/Professionals/Register/RegisterProfessionalUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Mappers;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.Professionals.Register
{
    /// <summary>
    /// Caso de uso responsável por orquestrar o fluxo de registro de um novo profissional.
    /// </summary>
    /// <remarks>
    /// Este caso de uso aplica as regras de negócio necessárias para criação de profissionais,
    /// incluindo licença duplicada e conversão de DTOs em entidades e value objects
    /// de domínio. A persistência efetiva é delegada ao repositório de profissionais.
    /// </remarks>
    public sealed class RegisterProfessionalUseCase
    {
        private readonly IProfessionalRepository _professionalRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de registro de profissional.
        /// </summary>
        /// <param name="professionalRepository">Repositório de profissionais utilizado para acesso a dados.</param>
        public RegisterProfessionalUseCase(IProfessionalRepository professionalRepository)
        {
            _professionalRepository = professionalRepository;
        }

        /// <summary>
        /// Executa o fluxo de registro de um novo profissional com base nos dados fornecidos.
        /// </summary>
        /// <param name="request">Dados de entrada necessários para registrar o profissional.</param>
        /// <returns>Um <see cref="RegisterProfessionalResponse"/> contendo o identificador do profissional criado.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando já existe um profissional cadastrado com a Licença informada
        /// </exception>
        public async Task<RegisterProfessionalResponse> Handle(RegisterProfessionalRequest request)
        {
            // Conversão dos dados do DTO para Value Objects de domínio
            var email = new Email(request.Email);
            var password = Password.Create(request.Password);
            var phone = new Phone(request.Phone);
            var policy = SchedulePolicyMapper.ToDomain(request.SchedulePolicy);
            var license = new ProfessionalLicense(request.License);

            // Regra de negócio: não permitir licença duplicada
            bool licenseAlreadyExists = await _professionalRepository.ExistsByProfessionalLicenseAsync(license);

            if (licenseAlreadyExists)
            {
                throw new InvalidOperationException("Já existe um profissional cadastrado com a licença informada.");
            }

            // Regra de negócio: não permitir Email duplicado
            bool emailAlreadyExists = await _professionalRepository.ExistsByEmailAsync(email);

            if (emailAlreadyExists)
            {
                throw new InvalidOperationException("Já existe um profissional cadastrado com o Email informado.");
            }

            // Construção da entidade de domínio Professional
            var professional = new Professional
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = email,
                Password = password,
                Phone = phone,
                Status = UserStatus.Active,
                Availability = Availability.Available,
                License = license,
                ProfessionalSchedule = new ProfessionalSchedule
                {
                    SchedulePolicy = policy
                }
            };

            // Garante que veio ao menos um consent
            if (request.Consents is null || !request.Consents.Any())
            {
                throw new InvalidOperationException("É obrigatório informar ao menos um consentimento.");
            }

            // Associação dos consentimentos fornecidos ao profissional
            foreach (var dto in request.Consents)
            {
                var consent = ConsentMapper.ToDomain(dto);
                professional.Consents.Add(consent);
            }

            // Usa o método do model para validar consentimento ativo
            var treatmentConsent = professional.GetActiveConsent(ConsentScope.Treatment);

            if (treatmentConsent is null)
            {
                throw new InvalidOperationException(
                    "O profissional não possui um consentimento de tratamento ativo."
                );
            }

            // Persistência
            await _professionalRepository.AddAsync(professional);

            return new RegisterProfessionalResponse(professional.Id);
        }
    }
}
