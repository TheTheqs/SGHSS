// Interface/Middlewares/GlobalExceptionHandlingMiddleware.cs

using Microsoft.EntityFrameworkCore;
using Npgsql;
using SGHSS.Interface.Models;
using System.Net;

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

                case DbUpdateException dbEx
                    when dbEx.InnerException is PostgresException pgEx
                         && pgEx.SqlState == "22001":
                    // 22001 = string data right truncation (valor muito longo para o campo)
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = "Um dos campos enviados excede o tamanho máximo permitido.";

                    // Loga com mais detalhe para você, mas sem expor pro cliente
                    _logger.LogError(dbEx,
                        "Erro de tamanho de campo ao acessar o banco de dados. SqlState: {SqlState}, Message: {PgMessage}",
                        pgEx.SqlState,
                        pgEx.MessageText);
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
                Message = message
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
