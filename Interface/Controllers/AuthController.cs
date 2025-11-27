// Interface/Controllers/AuthController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Authentication;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por expor o endpoint de autenticação de usuários,
    /// permitindo a obtenção de um token JWT a partir de credenciais válidas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticateUserUseCase _authenticateUserUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de autenticação.
        /// </summary>
        /// <param name="authenticateUserUseCase">
        /// Caso de uso responsável por autenticar usuários e gerar o token de acesso.
        /// </param>
        public AuthController(AuthenticateUserUseCase authenticateUserUseCase)
        {
            _authenticateUserUseCase = authenticateUserUseCase;
        }

        /// <summary>
        /// Autentica um usuário com base nas credenciais fornecidas e retorna
        /// um token JWT para acesso às demais rotas protegidas da API.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo e-mail e senha em texto puro.
        /// </param>
        /// <returns>
        /// Um <see cref="AuthenticateUserResponse"/> contendo o token gerado
        /// e as informações principais do usuário autenticado, caso as credenciais
        /// sejam válidas.
        /// </returns>
        /// <remarks>
        /// Este endpoint é público (não requer autenticação prévia),
        /// pois é justamente o responsável por gerar o token de acesso.
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthenticateUserResponse>> Login(
            [FromBody] AuthenticateUserRequest request)
        {
            try
            {
                var response = await _authenticateUserUseCase.Handle(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Para credenciais inválidas ou usuário inativo, retornamos 401.
                return Unauthorized(new
                {
                    error = ex.Message
                });
            }
        }
    }
}
