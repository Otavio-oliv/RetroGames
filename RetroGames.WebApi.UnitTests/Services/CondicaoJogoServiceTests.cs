
using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;
using Xunit;

namespace RetroGames.WebApi.UnitTests.Services;

public class CondicaoJogoServiceTests
{
    [Fact]
    public async Task ListarTodosAsync_RetornaCondicoesDoRepositorio()
    {
        // Arrange
        var condicoes = new List<CondicaoJogo>
        {
            new() { Id = 1, Descricao = "Novo" },
            new() { Id = 2, Descricao = "Usado" }
        };

        var condicaoRepository = new Mock<ICondicaoJogoRepository>();

        condicaoRepository
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(condicoes);

        var repositorios = new Mock<IRepositorySet>();

        repositorios
            .SetupGet(r => r.CondicaoJogo)
            .Returns(condicaoRepository.Object);

        var service = new CondicaoJogoService(repositorios.Object);

        // Act
        var resultado = await service.ListarTodosAsync();

        // Assert
        Assert.Equal(condicoes, resultado);

        condicaoRepository.Verify(
            r => r.ListarTodosAsync(),
            Times.Once);
    }
}
