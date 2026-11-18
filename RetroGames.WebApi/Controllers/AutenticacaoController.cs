
using System.Security.Claims;
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

    public AutenticacaoController(
        IAutenticacaoService autenticacaoService)
    {
        _autenticacaoService = autenticacaoService;
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

        return Ok(
            new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>
            {
                ResponseCode = ResponseCodes.SUCESSO,
                Data = new UsuarioLogadoResponseApiModel
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Perfil = perfil
                }
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
