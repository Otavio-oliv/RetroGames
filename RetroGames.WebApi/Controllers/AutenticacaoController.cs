
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Services.Interfaces;
using RetroGames.WebApi.ApiModels.Autenticacao;
using RetroGames.WebApi.ApiModels.Responses;
using RetroGames.WebApi.Extensions;

namespace RetroGames.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacaoService;
    private readonly IAntiforgery _antiforgery;

    public AutenticacaoController(
        IAutenticacaoService autenticacaoService,
        IAntiforgery antiforgery)
    {
        _autenticacaoService = autenticacaoService;
        _antiforgery = antiforgery;
    }

    [AllowAnonymous]
    [HttpGet("csrf-token")]
    public ActionResult<RetroGamesResponseApiModel<object>> ObterTokenCsrf()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(
            new RetroGamesResponseApiModel<object>
            {
                ResponseCode = ResponseCodes.SUCESSO,
                Data = new
                {
                    token = tokens.RequestToken
                }
            });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>>> Login(
        [FromBody] LoginRequestApiModel request)
    {
        var usuario = await _autenticacaoService.AutenticarAsync(
            request.Email,
            request.Senha);

        if (usuario == null)
        {
            return Unauthorized(
                new RetroGamesResponseApiModel<object>
                {
                    ResponseCode = ResponseCodes.CREDENCIAIS_INVALIDAS,
                    Data = null,
                    ErrorMessage = "E-mail ou senha inválidos."
                });
        }

        var perfil = ObterNomePerfil(usuario.IdPerfil);

        await CriarSessaoAsync(usuario, perfil);

        return Ok(
            new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>
            {
                ResponseCode = ResponseCodes.SUCESSO,
                Data = CriarRespostaUsuario(usuario, perfil)
            });
    }

    [AllowAnonymous]
    [HttpPost("cadastrar")]
    public async Task<ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>>> Cadastrar(
        [FromBody] CadastroRequestApiModel request)
    {
        var usuario = await _autenticacaoService.CadastrarAsync(
            request.Nome,
            request.Email,
            request.Senha);

        var perfil = ObterNomePerfil(usuario.IdPerfil);

        await CriarSessaoAsync(usuario, perfil);

        return StatusCode(
            StatusCodes.Status201Created,
            new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>
            {
                ResponseCode = ResponseCodes.SUCESSO,
                Data = CriarRespostaUsuario(usuario, perfil)
            });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<RetroGamesResponseApiModel<object>>> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(
            new RetroGamesResponseApiModel<object>
            {
                ResponseCode = ResponseCodes.SUCESSO,
                Data = null
            });
    }

    [Authorize]
    [HttpGet("eu")]
    public ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>> Eu()
    {
        return Ok(
            new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>
            {
                ResponseCode = ResponseCodes.SUCESSO,
                Data = new UsuarioLogadoResponseApiModel
                {
                    Id = User.ObterIdUsuario(),
                    Nome = User.ObterNomeUsuario(),
                    Email = User.ObterEmailUsuario(),
                    Perfil = User.ObterPerfilUsuario()
                }
            });
    }

    private async Task CriarSessaoAsync(
        Usuario usuario,
        string perfil)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, perfil)
        };

        var identidade = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identidade);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);
    }

    private static UsuarioLogadoResponseApiModel CriarRespostaUsuario(
        Usuario usuario,
        string perfil)
    {
        return new UsuarioLogadoResponseApiModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = perfil
        };
    }

    private static string ObterNomePerfil(int idPerfil)
    {
        return idPerfil switch
        {
            PerfilIds.Administrador => Perfis.Administrador,
            PerfilIds.Cliente => Perfis.Cliente,
            _ => string.Empty
        };
    }
}
