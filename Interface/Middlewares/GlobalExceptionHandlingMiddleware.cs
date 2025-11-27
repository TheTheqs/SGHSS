// Interface/Middlewares/GlobalExceptionHandlingMiddleware.cs

using System.Net;
using SGHSS.Interface.Models;

namespace SGHSS.Interface.Middlewares
{
    /// <summary>
    /// Middleware responsável por capturar exceções não tratadas
    /// e convertê-las em respostas HTTP padronizadas.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Valor padrão
            var statusCode = (int)HttpStatusCode.InternalServerError;
            string message;

            switch (exception)
            {
                case ArgumentException argEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    // Mantém a mensagem original da exceção
                    message = argEx.Message;
                    break;

                

                default:
                    // Para erros inesperados
                    _logger.LogError(exception, "Erro inesperado durante o processamento da requisição.");
                    message = "Ocorreu um erro interno no servidor.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message // aqui já está a mensagem original do ArgumentException
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
