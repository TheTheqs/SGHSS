// Interface/Models/ErrorResponse.cs (por exemplo)
namespace SGHSS.Interface.Models
{
    /// <summary>
    /// Representa o payload padrão de erro retornado pela API.
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }

        /// <summary>
        /// Mensagem de erro vinda diretamente da exceção de origem.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
