
using Microsoft.AspNetCore.Authentication.Cookies;
using RetroGames.WebApi.ApiModels.Responses;

namespace RetroGames.WebApi.Configuration;

public static class AutenticacaoConfig
{
    public static IServiceCollection AddAutenticacao(
        this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "retrogames.sessao";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode =
                        StatusCodes.Status401Unauthorized;

                    return context.Response.WriteAsJsonAsync(
                        new RetroGamesResponseApiModel<object>
                        {
                            ResponseCode = ResponseCodes.NAO_AUTENTICADO,
                            Data = null,
                            ErrorMessage = "Faça login para continuar."
                        });
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode =
                        StatusCodes.Status403Forbidden;

                    return context.Response.WriteAsJsonAsync(
                        new RetroGamesResponseApiModel<object>
                        {
                            ResponseCode = ResponseCodes.ACESSO_NEGADO,
                            Data = null,
                            ErrorMessage = "Você não tem permissão para esta operação."
                        });
                };
            });

        return services;
    }
}
