using Microsoft.AspNetCore.Identity;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;
using RetroGames.Services.Interfaces;

namespace RetroGames.Services.Services;

public class AutenticacaoService : IAutenticacaoService
{
    private readonly IRepositorySet _repositorySet;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AutenticacaoService(
        IRepositorySet repositorySet,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _repositorySet = repositorySet;
        _passwordHasher = passwordHasher;
    }

    public async Task<Usuario?> AutenticarAsync(
        string email,
        string senha)
    {
        var usuario = await _repositorySet.Usuario.BuscarPorEmailAsync(email);

        if (usuario == null || !usuario.Ativo)
            return null;

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.SenhaHash,
            senha);

        if (resultado == PasswordVerificationResult.Failed)
            return null;

        return usuario;
    }

    public async Task GarantirAdministradorInicialAsync(
        string email,
        string senha)
    {
        if (await _repositorySet.Usuario.ExisteAdministradorAsync())
            return;

        var administrador = new Usuario
        {
            Nome = "Administrador",
            Email = email,
            Ativo = true,
            IdPerfil = PerfilIds.Administrador
        };

        administrador.SenhaHash = _passwordHasher.HashPassword(
            administrador,
            senha);

        await _repositorySet.Usuario.InserirAsync(administrador);
    }
}
