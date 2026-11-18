
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RetroGames.Model.Constantes;

namespace RetroGames.WebApi.Configuration;

public static class AutorizacaoConfig
{
    public static IServiceCollection AddAutorizacao(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(
                Politicas.Administrador,
                policy => policy.RequireRole(Perfis.Administrador));
        });

        return services;
    }
}
