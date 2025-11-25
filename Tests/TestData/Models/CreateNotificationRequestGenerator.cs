// Tests/TestData/Models/CreateNotificationRequestGenerator.cs

using SGHSS.Application.UseCases.Notifications.Create;
using SGHSS.Domain.Enums;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para geração de dados de entrada do caso de uso de criação
    /// de notificações (<see cref="CreateNotificationRequest"/>), fornecendo valores
    /// fictícios porém válidos para cenários de teste.
    /// </summary>
    public static class CreateNotificationRequestGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um <see cref="CreateNotificationRequest"/> completo com dados fictícios,
        /// permitindo a sobrescrita de qualquer campo para cenários específicos de teste.
        /// </summary>
        /// <param name="providedRecipientId">
        /// Identificador de usuário a ser utilizado como destinatário. Caso seja <c>null</c>,
        /// um novo <see cref="Guid"/> será gerado.
        /// </param>
        /// <param name="providedChannel">
        /// Canal de notificação a ser utilizado. Caso não informado, um valor aleatório
        /// entre os disponíveis no enum <see cref="NotificationChannel"/> será selecionado.
        /// </param>
        /// <param name="providedMessage">
        /// Mensagem da notificação. Caso não informada ou vazia, uma mensagem fictícia
        /// e verossímil será gerada.
        /// </param>
        /// <returns>
        /// Instância preenchida de <see cref="CreateNotificationRequest"/> pronta para uso em testes.
        /// </returns>
        public static CreateNotificationRequest Generate(
            Guid? providedRecipientId = null,
            NotificationChannel? providedChannel = null,
            string? providedMessage = null
        )
        {
            Guid recipientId = providedRecipientId ?? Guid.NewGuid();
            NotificationChannel channel = providedChannel ?? GenerateRandomChannel();
            string message = string.IsNullOrWhiteSpace(providedMessage)
                ? GenerateMessage()
                : providedMessage!;

            return new CreateNotificationRequest
            {
                RecipientId = recipientId,
                Channel = channel,
                Message = message
            };
        }

        /// <summary>
        /// Seleciona aleatoriamente um canal de notificação válido.
        /// </summary>
        private static NotificationChannel GenerateRandomChannel()
        {
            Array values = Enum.GetValues(typeof(NotificationChannel));
            return (NotificationChannel)values.GetValue(Rng.Next(values.Length))!;
        }

        /// <summary>
        /// Gera uma mensagem fictícia e coerente para testes de notificação.
        /// </summary>
        private static string GenerateMessage()
        {
            string[] samples =
            {
                "Seu agendamento foi confirmado com sucesso.",
                "Há novos documentos disponíveis para consulta.",
                "Atualização importante: verifique seu prontuário médico.",
                "Você recebeu uma nova mensagem do profissional de saúde.",
                "Lembrete: retorno agendado para amanhã às 14h.",
                "Notificação automática: alterações realizadas em sua conta."
            };

            return samples[Rng.Next(samples.Length)];
        }
    }
}
