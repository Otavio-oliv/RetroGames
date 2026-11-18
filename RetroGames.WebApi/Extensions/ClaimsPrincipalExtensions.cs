
using System.Security.Claims;

namespace RetroGames.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int ObterIdUsuario(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(valor, out var id))
        {
            throw new InvalidOperationException(
                "Não foi possível identificar o usuário autenticado.");
        }

        return id;
    }

    public static string ObterNomeUsuario(this ClaimsPrincipal usuario)
    {
        return usuario.FindFirst(ClaimTypes.Name)?.Value
            ?? string.Empty;
    }

    public static string ObterEmailUsuario(this ClaimsPrincipal usuario)
    {
        return usuario.FindFirst(ClaimTypes.Email)?.Value
            ?? string.Empty;
    }

    public static string ObterPerfilUsuario(this ClaimsPrincipal usuario)
    {
        return usuario.FindFirst(ClaimTypes.Role)?.Value
            ?? string.Empty;
    }
}
