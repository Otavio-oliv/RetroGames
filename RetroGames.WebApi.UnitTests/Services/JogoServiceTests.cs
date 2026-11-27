
using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

public class JogoServiceTests
{
    private readonly Mock<IRepositorySet> _repositorios;
    private readonly Mock<IJogoRepository> _repositorioJogo;
    private readonly Mock<IJogoGeneroRepository> _repositorioJogoGenero;
    private readonly Mock<IPlataformaRepository> _repositorioPlataforma;
    private readonly Mock<IGeneroRepository> _repositorioGenero;
    private readonly Mock<ICondicaoJogoRepository> _repositorioCondicao;
    private readonly Mock<IUsuarioAtualService> _usuarioAtual;

    public JogoServiceTests()
    {
        _repositorios = new Mock<IRepositorySet>();
        _repositorioJogo = new Mock<IJogoRepository>();
        _repositorioJogoGenero = new Mock<IJogoGeneroRepository>();
        _repositorioPlataforma = new Mock<IPlataformaRepository>();
        _repositorioGenero = new Mock<IGeneroRepository>();
        _repositorioCondicao = new Mock<ICondicaoJogoRepository>();
        _usuarioAtual = new Mock<IUsuarioAtualService>();

        _repositorios
            .SetupGet(r => r.Jogo)
            .Returns(_repositorioJogo.Object);

        _repositorios
            .SetupGet(r => r.JogoGenero)
            .Returns(_repositorioJogoGenero.Object);

        _repositorios
            .SetupGet(r => r.Plataforma)
            .Returns(_repositorioPlataforma.Object);

        _repositorios
            .SetupGet(r => r.Genero)
            .Returns(_repositorioGenero.Object);

        _repositorios
            .SetupGet(r => r.CondicaoJogo)
            .Returns(_repositorioCondicao.Object);

        _usuarioAtual
            .SetupGet(u => u.EhAdministrador)
            .Returns(false);
    }

    private JogoService CriarService()
    {
        return new JogoService(
            _repositorios.Object,
            _usuarioAtual.Object);
    }

    private static Jogo CriarJogo()
    {
        return new Jogo
        {
            Nome = "Super Mario Bros",
            Descricao = "Jogo de plataforma",
            AnoLancamento = 1985,
            Preco = 100,
            Estoque = 5,
            IdPlataforma = 1,
            IdCondicaoJogo = 1
        };
    }

    [Fact]
    public async Task Inserir_PlataformaInexistente_LancaErroENaoInsere()
    {
        // Arrange
        var service = CriarService();
        var jogo = CriarJogo();

        _repositorioJogo
            .Setup(r => r.ExisteComNomeAsync("Super Mario Bros", null))
            .ReturnsAsync(false);

        _repositorioPlataforma
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync((Plataforma?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(jogo, new[] { 1 }));

        // Assert
        Assert.Equal(
            BusinessErrors.PLATAFORMA_NAO_ENCONTRADA,
            excecao.Erro);

        _repositorioJogo.Verify(
            r => r.InserirAsync(It.IsAny<Jogo>()),
            Times.Never);
    }

    [Fact]
    public async Task Inserir_CondicaoInexistente_LancaErroENaoInsere()
    {
        // Arrange
        var service = CriarService();
        var jogo = CriarJogo();

        _repositorioJogo
            .Setup(r => r.ExisteComNomeAsync("Super Mario Bros", null))
            .ReturnsAsync(false);

        _repositorioPlataforma
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(new Plataforma
            {
                Id = 1,
                Nome = "NES"
            });

        _repositorioCondicao
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(new List<CondicaoJogo>());

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(jogo, new[] { 1 }));

        // Assert
        Assert.Equal(
            BusinessErrors.CONDICAO_NAO_ENCONTRADA,
            excecao.Erro);

        _repositorioJogo.Verify(
            r => r.InserirAsync(It.IsAny<Jogo>()),
            Times.Never);
    }

    [Fact]
    public async Task Inserir_GeneroInexistente_LancaErroENaoInsere()
    {
        // Arrange
        var service = CriarService();
        var jogo = CriarJogo();

        _repositorioJogo
            .Setup(r => r.ExisteComNomeAsync("Super Mario Bros", null))
            .ReturnsAsync(false);

        _repositorioPlataforma
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(new Plataforma
            {
                Id = 1,
                Nome = "NES"
            });

        _repositorioCondicao
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(new List<CondicaoJogo>
            {
                new()
                {
                    Id = 1,
                    Descricao = "Novo"
                }
            });

        _repositorioGenero
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(new List<Genero>
            {
                new()
                {
                    Id = 1,
                    Nome = "Plataforma"
                }
            });

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(jogo, new[] { 999 }));

        // Assert
        Assert.Equal(
            BusinessErrors.GENERO_NAO_ENCONTRADO,
            excecao.Erro);

        _repositorioJogo.Verify(
            r => r.InserirAsync(It.IsAny<Jogo>()),
            Times.Never);
    }

    [Fact]
    public async Task Listar_ComFiltroDeGenero_PassaSomenteIdsDoGenero()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogoGenero
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(new List<JogoGenero>
            {
                new() { IdJogo = 10, IdGenero = 5 },
                new() { IdJogo = 11, IdGenero = 3 },
                new() { IdJogo = 12, IdGenero = 5 }
            });

        _repositorioJogo
            .Setup(r => r.ListarAsync(
                null,
                null,
                It.Is<IReadOnlyCollection<int>?>(
                    ids => ids != null &&
                           ids.OrderBy(id => id).SequenceEqual(new[] { 10, 12 })),
                1,
                10,
                false))
            .ReturnsAsync((new List<Jogo>(), 0));

        // Act
        var resultado = await service.ListarAsync(idGenero: 5);

        // Assert
        Assert.Empty(resultado.Jogos);
        Assert.Equal(0, resultado.Total);

        _repositorioJogo.Verify(
            r => r.ListarAsync(
                null,
                null,
                It.Is<IReadOnlyCollection<int>?>(
                    ids => ids != null &&
                           ids.OrderBy(id => id).SequenceEqual(new[] { 10, 12 })),
                1,
                10,
                false),
            Times.Once);
    }

    [Fact]
    public async Task Listar_SemFiltroDeGenero_NaoConsultaVinculos()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .Setup(r => r.ListarAsync(
                null,
                null,
                null,
                1,
                10,
                false))
            .ReturnsAsync((new List<Jogo>(), 0));

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Empty(resultado.Jogos);
        Assert.Equal(0, resultado.Total);

        _repositorioJogoGenero.Verify(
            r => r.ListarTodosAsync(),
            Times.Never);

        _repositorioJogo.Verify(
            r => r.ListarAsync(
                null,
                null,
                null,
                1,
                10,
                false),
            Times.Once);
    }

    [Fact]
    public async Task Listar_ComJogos_MontaDetalhesComPlataformaCondicaoEGeneros()
    {
        // Arrange
        var service = CriarService();

        var jogo = new Jogo
        {
            Id = 10,
            Nome = "Super Mario Bros",
            IdPlataforma = 1,
            IdCondicaoJogo = 2,
            Ativo = true
        };

        _repositorioJogo
            .Setup(r => r.ListarAsync(
                null,
                null,
                null,
                1,
                10,
                false))
            .ReturnsAsync((new List<Jogo> { jogo }, 1));

        _repositorioJogoGenero
            .Setup(r => r.ListarPorJogosAsync(
                It.Is<IReadOnlyCollection<int>>(
                    ids => ids.Contains(10))))
            .ReturnsAsync(new List<JogoGenero>
            {
                new() { IdJogo = 10, IdGenero = 1 },
                new() { IdJogo = 10, IdGenero = 2 }
            });

        _repositorioPlataforma
            .Setup(r => r.ListarAsync())
            .ReturnsAsync(new List<Plataforma>
            {
                new()
                {
                    Id = 1,
                    Nome = "NES"
                }
            });

        _repositorioCondicao
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(new List<CondicaoJogo>
            {
                new()
                {
                    Id = 2,
                    Descricao = "Usado"
                }
            });

        _repositorioGenero
            .Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(new List<Genero>
            {
                new()
                {
                    Id = 1,
                    Nome = "Plataforma"
                },
                new()
                {
                    Id = 2,
                    Nome = "Aventura"
                }
            });

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Equal(1, resultado.Total);
        Assert.Single(resultado.Jogos);

        var detalhe = resultado.Jogos[0];

        Assert.Equal("Super Mario Bros", detalhe.Jogo.Nome);
        Assert.Equal("NES", detalhe.NomePlataforma);
        Assert.Equal("Usado", detalhe.DescricaoCondicao);

        Assert.Equal(2, detalhe.Generos.Count);
        Assert.Contains(detalhe.Generos, g => g.Nome == "Plataforma");
        Assert.Contains(detalhe.Generos, g => g.Nome == "Aventura");
    }

    [Fact]
    public async Task BuscarPorId_JogoInexistente_LancaNaoEncontrado()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(999))
            .ReturnsAsync((Jogo?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.BuscarPorIdAsync(999));

        // Assert
        Assert.Equal(
            BusinessErrors.JOGO_NAO_ENCONTRADO,
            excecao.Erro);
    }

    [Fact]
    public async Task Desativar_JogoInexistente_LancaNaoEncontradoENaoDesativa()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(999))
            .ReturnsAsync((Jogo?)null);

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.DesativarAsync(999));

        // Assert
        Assert.Equal(
            BusinessErrors.JOGO_NAO_ENCONTRADO,
            excecao.Erro);

        _repositorioJogo.Verify(
            r => r.DesativarAsync(999),
            Times.Never);
    }
}
