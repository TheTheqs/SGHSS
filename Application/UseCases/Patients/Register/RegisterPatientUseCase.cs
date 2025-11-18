// Application/UseCases/Patients/Register/RegisterPatientUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using System.Threading;

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
        public async Task<RegisterPatientResponse> Handle(
            RegisterPatientRequest request,
            CancellationToken cancellationToken = default)
        {
            // Conversão dos dados do DTO para Value Objects de domínio
            var email = new Email(request.Email);
            var phone = new Phone(request.Phone);
            var cpf = new Cpf(request.Cpf);

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


            Address address = MapAddress(request.Address);

            // Construção da entidade de domínio Patient
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = email,
                Phone = phone,
                Cpf = cpf,
                BirthDate = request.BirthDate,
                Sex = request.Sex,
                Address = address,
                EmergencyContactName = request.EmergencyContactName
                // Status, MedicalRecord e relacionamentos adicionais
                // podem ser tratados em outros fluxos/casos de uso.
            };

            // Persistência (somente adiciona ao repositório; o commit/SaveChanges
            // Tratado Futuramente com UnitOfWork ou similar)
            await _patientRepository.AddAsync(patient);

            return new RegisterPatientResponse(patient.Id);
        }

        /// <summary>
        /// Mapeia o <see cref="AddressDto"/> recebido pela camada de interface
        /// para o Value Object <see cref="Address"/> do domínio.
        /// </summary>
        /// <param name="dto">DTO contendo os dados de endereço.</param>
        /// <returns>Instância de <see cref="Address"/> construída a partir do DTO.</returns>
        private static Address MapAddress(AddressDto dto)
        {
            return new Address(
                street: dto.Street,
                number: dto.Number,
                city: dto.City,
                state: dto.State,
                cep: dto.Cep,
                district: dto.District,
                complement: dto.Complement,
                country: dto.Country
            );
        }
    }
}
