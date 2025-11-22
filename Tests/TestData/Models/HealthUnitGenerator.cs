// Tests/TestData/Models/HealthUnitGenerator.cs

using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.ValueObjects;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para geração de dados de entrada de unidade de saúde
    /// (RegisterHealthUnitRequest) com valores válidos para uso em testes.
    /// </summary>
    public static class HealthUnitGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Mapeia o tipo de unidade de saúde para um prefixo de nome amigável.
        /// </summary>
        private static readonly Dictionary<HealthUnitType, string> HealthUnitTypeNamePrefix = new()
        {
            { HealthUnitType.Hospital,   "Hospital" },
            { HealthUnitType.Clinic,     "Clínica" },
            { HealthUnitType.Laboratory, "Laboratório" }
        };

        /// <summary>
        /// Gera um RegisterHealthUnitRequest completo com dados fictícios,
        /// porém válidos de acordo com as regras dos Value Objects.
        /// </summary>
        /// <param name="providedCnpj">
        /// CNPJ fornecido externamente. Se vazio ou nulo, um CNPJ válido será gerado.
        /// </param>
        /// <returns>Instância de RegisterHealthUnitRequest preenchida.</returns>
        public static RegisterHealthUnitRequest GenerateHealthUnit(string providedCnpj = "")
        {
            // Tipo de unidade sorteado a partir do enum
            HealthUnitType type = GetRandomHealthUnitType();

            // Prefixo do nome baseado no tipo
            string prefix = HealthUnitTypeNamePrefix[type];

            // Nome “humano” para compor o nome da unidade
            string personName = NameGenerator.GetFullName();

            // Nome final da unidade (ex.: "Hospital João da Silva")
            string unitName = $"{prefix} {personName}";

            // CNPJ válido
            string cnpj = string.IsNullOrWhiteSpace(providedCnpj)
                ? CnpjGenerator.GenerateCnpj(withMask: false)
                : providedCnpj;

            // Telefone válido
            string phone = PhoneGenerator.GeneratePhone();

            // Endereço fictício, porém válido
            AddressDto address = AddressGenerator.GenerateAddress();

            return new RegisterHealthUnitRequest
            {
                Name = unitName,
                Cnpj = cnpj,
                Phone = phone,
                Address = address,
                Type = type
            };
        }

        /// <summary>
        /// Sorteia um valor válido do enum HealthUnitType.
        /// </summary>
        private static HealthUnitType GetRandomHealthUnitType()
        {
            var values = Enum.GetValues(typeof(HealthUnitType));
            return (HealthUnitType)values.GetValue(Rng.Next(values.Length))!;
        }
    }
}
