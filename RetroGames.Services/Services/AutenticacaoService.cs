
using Microsoft.AspNetCore.Identity;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository.Exceptions;
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

    public async Task<Usuario> CadastrarAsync(
        string nome,
        string email,
        string senha)
    {
        var nomeNormalizado = nome.Trim();
        var emailNormalizado = email.Trim().ToLowerInvariant();

        var usuarioExistente =
            await _repositorySet.Usuario.BuscarPorEmailAsync(emailNormalizado);

        if (usuarioExistente != null)
        {
            throw new BusinessRuleException(
                BusinessErrors.EMAIL_JA_CADASTRADO);
        }

        var usuario = new Usuario
        {
            Nome = nomeNormalizado,
            Email = emailNormalizado,
            Ativo = true,
            IdPerfil = PerfilIds.Cliente
        };

        usuario.SenhaHash = _passwordHasher.HashPassword(
            usuario,
            senha);

        try
        {
            usuario.Id = await _repositorySet.Usuario.InserirAsync(usuario);
        }
        catch (RegistroDuplicadoException)
        {
            // Protege também contra duas tentativas simultâneas
            // de cadastro com o mesmo e-mail.
            throw new BusinessRuleException(
                BusinessErrors.EMAIL_JA_CADASTRADO);
        }

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
