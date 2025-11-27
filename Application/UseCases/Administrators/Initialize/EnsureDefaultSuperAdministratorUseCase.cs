// Application/UseCases/Administrators/Initialize/EnsureDefaultSuperAdministratorUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Administrators.Initialize
{
    /// <summary>
    /// Caso de uso responsável por garantir que exista um Administrador do tipo SUPER
    /// pré-configurado no sistema. Se não existir um administrador com o e-mail padrão,
    /// este caso de uso registra um novo automaticamente.
    /// </summary>
    /// <remarks>
    /// IMPORTANTE: As credenciais e dados padrão a seguir estão expostos apenas
    /// para fins acadêmicos (TCC / ambiente de teste).
    /// Em um ambiente real, estes valores devem ser lidos de variáveis de ambiente
    /// ou de um cofre de segredos seguro.
    /// </remarks>
    public sealed class EnsureDefaultSuperAdministratorUseCase
    {
        private readonly IAdministratorRepository _administratorRepository;

        public EnsureDefaultSuperAdministratorUseCase(IAdministratorRepository administratorRepository)
        {
            _administratorRepository = administratorRepository;
        }

        /// <summary>
        /// Garante que exista um administrador SUPER no sistema. Caso não exista
        /// um administrador com o e-mail padrão, cria um novo registro.
        /// </summary>
        public async Task Handle()
        {
            // ⚠️ ATENÇÃO:
            // Estes valores são APENAS para TCC / testes locais.
            // Em produção, use variáveis de ambiente ou configuração segura.
            const string defaultName = "Administrador Padrão SGHSS";
            const string defaultEmail = "super.admin@sghss.local";
            const string defaultPassword = "Admin12345";
            const string defaultPhone = "11987654321";

            // Termos e hash fictícios, porém válidos
            const string defaultTermVersion = "v1-super-admin-bootstrap";

            // 64 caracteres hexadecimais válidos (compatível com HashDigest)
            const string defaultTermHash =
                "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

            // Verifica se já existe administrador com esse e-mail
            var emailVo = new Domain.ValueObjects.Email(defaultEmail);
            bool exists = await _administratorRepository.ExistsByEmailAsync(emailVo);

            if (exists)
            {
                // Já existe o super admin padrão, nada a fazer.
                return;
            }

            // Monta o request de registro de administrador SUPER
            var request = new RegisterAdministratorRequest
            {
                Name = defaultName,
                Email = defaultEmail,
                Password = defaultPassword,
                Phone = defaultPhone,
                AccessLevel = AccessLevel.Super
            };

            // Gera um consentimento para cada escopo
            var now = DateTimeOffset.UtcNow;

            request.Consents.Add(CreateConsent(
                scope: ConsentScope.Treatment,
                termVersion: defaultTermVersion,
                termHash: defaultTermHash,
                consentDate: now));

            request.Consents.Add(CreateConsent(
                scope: ConsentScope.Research,
                termVersion: defaultTermVersion,
                termHash: defaultTermHash,
                consentDate: now));

            request.Consents.Add(CreateConsent(
                scope: ConsentScope.Notification,
                termVersion: defaultTermVersion,
                termHash: defaultTermHash,
                consentDate: now));

            // Aproveita o mesmo caso de uso de registro que você já tem,
            // para garantir que TODAS as regras (consents, status, etc.) sejam aplicadas.
            var registerUseCase = new RegisterAdministratorUseCase(_administratorRepository);
            await registerUseCase.Handle(request);
        }

        /// <summary>
        /// Cria um ConsentDto válido para o escopo informado.
        /// </summary>
        private static ConsentDto CreateConsent(
            ConsentScope scope,
            string termVersion,
            string termHash,
            DateTimeOffset consentDate)
        {
            return new ConsentDto
            {
                Scope = scope,
                TermVersion = termVersion,
                Channel = ConsentChannel.Web,
                ConsentDate = consentDate,
                RevocationDate = null,
                TermHash = termHash
            };
        }
    }
}
