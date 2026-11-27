
using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;
using Xunit;

namespace RetroGames.WebApi.UnitTests.Services;

public class GeneroServiceTests
{
    [Fact]
    public async Task ListarTodosAsync_RetornaGenerosDoRepositorio()
    {
        // Arrange
        var generos = new List<Genero>
        {
            new() { Id = 1, Nome = "Plataforma" },
            new() { Id = 2, Nome = "Aventura" }
        };

        var generoRepository = new Mock<IGeneroRepository>();

        generoRepository
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(generos);

        var repositorios = new Mock<IRepositorySet>();

        repositorios
            .SetupGet(r => r.Genero)
            .Returns(generoRepository.Object);

        var service = new GeneroService(repositorios.Object);

        // Act
        var resultado = await service.ListarTodosAsync();

        // Assert
        Assert.Equal(generos, resultado);

        generoRepository.Verify(
            r => r.ListarTodosAsync(),
            Times.Once);
    }
}
