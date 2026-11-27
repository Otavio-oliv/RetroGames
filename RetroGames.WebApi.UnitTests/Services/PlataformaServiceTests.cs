
using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository.Exceptions;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

public class PlataformaServiceTests
{
    private readonly Mock<IPlataformaRepository> _repositorioPlataforma;
    private readonly Mock<IRepositorySet> _repositorios;

    public PlataformaServiceTests()
    {
        _repositorioPlataforma = new Mock<IPlataformaRepository>();
        _repositorios = new Mock<IRepositorySet>();

        _repositorios
            .SetupGet(r => r.Plataforma)
            .Returns(_repositorioPlataforma.Object);
    }

    private PlataformaService CriarService()
    {
        return new PlataformaService(_repositorios.Object);
    }

    [Fact]
    public async Task BuscarPorId_IdInexistente_LancaNaoEncontrada()
    {
        // Arrange
        var service = CriarService();

        _repositorioPlataforma
            .Setup(r => r.BuscarPorIdAsync(999))
            .ReturnsAsync((Plataforma?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.BuscarPorIdAsync(999));

        // Assert
        Assert.Equal(
            BusinessErrors.PLATAFORMA_NAO_ENCONTRADA,
            excecao.Erro);

        _repositorioPlataforma.Verify(
            r => r.BuscarPorIdAsync(999),
            Times.Once);
    }

    [Fact]
    public async Task Inserir_NomeDuplicado_LancaErroENaoInsere()
    {
        // Arrange
        var service = CriarService();

        var plataforma = new Plataforma
        {
            Nome = "NES",
            Fabricante = "Nintendo",
            AnoLancamento = 1983
        };

        _repositorioPlataforma
            .Setup(r => r.ExisteComNomeAsync("NES", null))
            .ReturnsAsync(true);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(plataforma));

        // Assert
        Assert.Equal(
            BusinessErrors.PLATAFORMA_NOME_DUPLICADO,
            excecao.Erro);

        _repositorioPlataforma.Verify(
            r => r.InserirAsync(It.IsAny<Plataforma>()),
            Times.Never);
    }

    [Fact]
    public async Task Inserir_RepositorioLancaDuplicidade_ConverteParaErroDeNegocio()
    {
        // Arrange
        var service = CriarService();

        var plataforma = new Plataforma
        {
            Nome = "Atari 2600",
            Fabricante = "Atari",
            AnoLancamento = 1977
        };

        _repositorioPlataforma
            .Setup(r => r.ExisteComNomeAsync("Atari 2600", null))
            .ReturnsAsync(false);

        _repositorioPlataforma
            .Setup(r => r.InserirAsync(plataforma))
            .ThrowsAsync(
                new RegistroDuplicadoException("Registro duplicado."));

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(plataforma));

        // Assert
        Assert.Equal(
            BusinessErrors.PLATAFORMA_NOME_DUPLICADO,
            excecao.Erro);

        _repositorioPlataforma.Verify(
            r => r.InserirAsync(plataforma),
            Times.Once);
    }

    [Fact]
    public async Task Inserir_PlataformaValida_RetornaIdGerado()
    {
        // Arrange
        var service = CriarService();

        var plataforma = new Plataforma
        {
            Nome = "  Mega Drive  ",
            Fabricante = "Sega",
            AnoLancamento = 1988
        };

        _repositorioPlataforma
            .Setup(r => r.ExisteComNomeAsync("Mega Drive", null))
            .ReturnsAsync(false);

        _repositorioPlataforma
            .Setup(r => r.InserirAsync(plataforma))
            .ReturnsAsync(42);

        // Act
        var resultado = await service.InserirAsync(plataforma);

        // Assert
        Assert.Equal(42, resultado.Id);
        Assert.Equal("Mega Drive", resultado.Nome);

        _repositorioPlataforma.Verify(
            r => r.InserirAsync(plataforma),
            Times.Once);
    }

    [Fact]
    public async Task Alterar_MantendoProprioNome_PermiteAlteracao()
    {
        // Arrange
        var service = CriarService();

        var existente = new Plataforma
        {
            Id = 1,
            Nome = "NES",
            Fabricante = "Nintendo",
            AnoLancamento = 1983
        };

        var plataforma = new Plataforma
        {
            Id = 1,
            Nome = "NES",
            Fabricante = "Nintendo",
            AnoLancamento = 1983
        };

        _repositorioPlataforma
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(existente);

        _repositorioPlataforma
            .Setup(r => r.ExisteComNomeAsync("NES", 1))
            .ReturnsAsync(false);

        _repositorioPlataforma
            .Setup(r => r.AlterarAsync(plataforma))
            .Returns(Task.CompletedTask);

        // Act
        await service.AlterarAsync(plataforma);

        // Assert
        _repositorioPlataforma.Verify(
            r => r.ExisteComNomeAsync("NES", 1),
            Times.Once);

        _repositorioPlataforma.Verify(
            r => r.AlterarAsync(plataforma),
            Times.Once);
    }

    [Fact]
    public async Task Excluir_PlataformaComJogos_LancaErroENaoExclui()
    {
        // Arrange
        var service = CriarService();

        var plataforma = new Plataforma
        {
            Id = 1,
            Nome = "NES",
            Fabricante = "Nintendo",
            AnoLancamento = 1983
        };

        _repositorioPlataforma
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(plataforma);

        _repositorioPlataforma
            .Setup(r => r.PossuiJogosAsync(1))
            .ReturnsAsync(true);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.ExcluirAsync(1));

        // Assert
        Assert.Equal(
            BusinessErrors.PLATAFORMA_POSSUI_JOGOS,
            excecao.Erro);

        _repositorioPlataforma.Verify(
            r => r.ExcluirAsync(1),
            Times.Never);
    }
}
