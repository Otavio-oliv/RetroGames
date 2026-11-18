using Microsoft.AspNetCore.Http;
using RetroGames.Services;
using RetroGames.WebApi.Extensions;

namespace RetroGames.WebApi.Services;

public class UsuarioAtualService : IUsuarioAtualService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtualService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? Contexto =>
        _httpContextAccessor.HttpContext;

    public bool EstaAutenticado =>
        Contexto?.User.Identity?.IsAuthenticated == true;

    public int IdUsuario =>
        EstaAutenticado
            ? Contexto!.User.ObterIdUsuario()
            : 0;

    public bool EhAdministrador =>
        EstaAutenticado &&
        Contexto!.User.IsInRole("Administrador");
}
