
using Microsoft.AspNetCore.Identity;
using Moq;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository.Exceptions;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;
using RetroGames.Services.Services;
using Xunit;

namespace RetroGames.WebApi.UnitTests.Services;

public class AutenticacaoServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositorioUsuario;
    private readonly Mock<IRepositorySet> _repositorios;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AutenticacaoServiceTests()
    {
        _repositorioUsuario = new Mock<IUsuarioRepository>();
        _repositorios = new Mock<IRepositorySet>();
        _passwordHasher = new PasswordHasher<Usuario>();

        _repositorios
            .SetupGet(r => r.Usuario)
            .Returns(_repositorioUsuario.Object);
    }

    private AutenticacaoService CriarService()
    {
        return new AutenticacaoService(
            _repositorios.Object,
            _passwordHasher);
    }

    [Fact]
    public async Task Cadastrar_UsuarioValido_CadastraComoClienteAtivo()
    {
        // Arrange
        var service = CriarService();

        _repositorioUsuario
            .Setup(r => r.BuscarPorEmailAsync("usuario@email.com"))
            .ReturnsAsync((Usuario?)null);

        _repositorioUsuario
            .Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(42);

        // Act
        var resultado = await service.CadastrarAsync(
            "Usuário Teste",
            "usuario@email.com",
            "Senha123");

        // Assert
        Assert.Equal(42, resultado.Id);
        Assert.Equal("Usuário Teste", resultado.Nome);
        Assert.Equal("usuario@email.com", resultado.Email);
        Assert.True(resultado.Ativo);
        Assert.Equal(PerfilIds.Cliente, resultado.IdPerfil);

        _repositorioUsuario.Verify(
            r => r.InserirAsync(It.IsAny<Usuario>()),
            Times.Once);
    }

    [Fact]
    public async Task Cadastrar_SenhaValida_ArmazenaHashEPermiteVerificacao()
    {
        // Arrange
        var service = CriarService();
        Usuario? usuarioInserido = null;

        _repositorioUsuario
            .Setup(r => r.BuscarPorEmailAsync("usuario@email.com"))
            .ReturnsAsync((Usuario?)null);

        _repositorioUsuario
            .Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(usuario => usuarioInserido = usuario)
            .ReturnsAsync(42);

        // Act
        await service.CadastrarAsync(
            "Usuário Teste",
            "usuario@email.com",
            "Senha123");

        // Assert
        Assert.NotNull(usuarioInserido);
        Assert.NotEqual("Senha123", usuarioInserido.SenhaHash);

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuarioInserido,
            usuarioInserido.SenhaHash,
            "Senha123");

        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            resultado);
    }

    [Fact]
    public async Task Cadastrar_EmailJaCadastrado_LancaErroENaoInsere()
    {
        // Arrange
        var service = CriarService();

        var usuarioExistente = new Usuario
        {
            Id = 10,
            Nome = "Outro Usuário",
            Email = "usuario@email.com",
            Ativo = true,
            IdPerfil = PerfilIds.Cliente
        };

        _repositorioUsuario
            .Setup(r => r.BuscarPorEmailAsync("usuario@email.com"))
            .ReturnsAsync(usuarioExistente);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CadastrarAsync(
                "Novo Usuário",
                "usuario@email.com",
                "Senha123"));

        // Assert
        Assert.Equal(
            BusinessErrors.EMAIL_JA_CADASTRADO,
            excecao.Erro);

        _repositorioUsuario.Verify(
            r => r.InserirAsync(It.IsAny<Usuario>()),
            Times.Never);
    }

    [Fact]
    public async Task Cadastrar_RepositorioLancaDuplicidade_ConverteParaErroDeNegocio()
    {
        // Arrange
        var service = CriarService();

        _repositorioUsuario
            .Setup(r => r.BuscarPorEmailAsync("usuario@email.com"))
            .ReturnsAsync((Usuario?)null);

        _repositorioUsuario
            .Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .ThrowsAsync(
                new RegistroDuplicadoException("Registro duplicado."));

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CadastrarAsync(
                "Usuário Teste",
                "usuario@email.com",
                "Senha123"));

        // Assert
        Assert.Equal(
            BusinessErrors.EMAIL_JA_CADASTRADO,
            excecao.Erro);

        _repositorioUsuario.Verify(
            r => r.InserirAsync(It.IsAny<Usuario>()),
            Times.Once);
    }

    [Fact]
    public async Task Cadastrar_DadosComEspacosExtras_NormalizaNomeEEmail()
    {
        // Arrange
        var service = CriarService();

        _repositorioUsuario
            .Setup(r => r.BuscarPorEmailAsync("usuario@email.com"))
            .ReturnsAsync((Usuario?)null);

        _repositorioUsuario
            .Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(42);

        // Act
        var resultado = await service.CadastrarAsync(
            "  Usuário Teste  ",
            "  USUARIO@EMAIL.COM  ",
            "Senha123");

        // Assert
        Assert.Equal("Usuário Teste", resultado.Nome);
        Assert.Equal("usuario@email.com", resultado.Email);
    }
}
