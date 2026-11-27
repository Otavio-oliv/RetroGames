
using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

public class PlataformaServiceTests
{
    private readonly Mock<IPlataformaRepository> _repositorioPlataforma;
    private readonly Mock<IRepositorySet> _repositorios;

    public PlataformaServiceTests()
    {
        // Cria os mocks dos repositórios.
        _repositorioPlataforma = new Mock<IPlataformaRepository>();
        _repositorios = new Mock<IRepositorySet>();

        // Configura o RepositorySet para devolver
        // o mock de plataformas.
        _repositorios
            .SetupGet(r => r.Plataforma)
            .Returns(_repositorioPlataforma.Object);
    }

    // Cria o Service utilizando os repositórios simulados.
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
}
