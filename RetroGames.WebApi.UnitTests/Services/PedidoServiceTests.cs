
using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

public class PedidoServiceTests
{
    private readonly Mock<IRepositorySet> _repositorios;
    private readonly Mock<IJogoRepository> _repositorioJogo;
    private readonly Mock<IPedidoRepository> _repositorioPedido;
    private readonly Mock<IItemPedidoRepository> _repositorioItemPedido;
    private readonly Mock<IUsuarioAtualService> _usuarioAtual;

    public PedidoServiceTests()
    {
        _repositorios = new Mock<IRepositorySet>();
        _repositorioJogo = new Mock<IJogoRepository>();
        _repositorioPedido = new Mock<IPedidoRepository>();
        _repositorioItemPedido = new Mock<IItemPedidoRepository>();
        _usuarioAtual = new Mock<IUsuarioAtualService>();

        _repositorios
            .SetupGet(r => r.Jogo)
            .Returns(_repositorioJogo.Object);

        _repositorios
            .SetupGet(r => r.Pedido)
            .Returns(_repositorioPedido.Object);

        _repositorios
            .SetupGet(r => r.ItemPedido)
            .Returns(_repositorioItemPedido.Object);

        _usuarioAtual
            .SetupGet(u => u.EstaAutenticado)
            .Returns(true);

        _usuarioAtual
            .SetupGet(u => u.IdUsuario)
            .Returns(42);
    }

    private PedidoService CriarService()
    {
        return new PedidoService(
            _repositorios.Object,
            _usuarioAtual.Object);
    }

    private static Jogo CriarJogo(
        int id = 1,
        decimal preco = 100,
        int estoque = 5,
        bool ativo = true)
    {
        return new Jogo
        {
            Id = id,
            Nome = "Super Mario Bros",
            Preco = preco,
            Estoque = estoque,
            Ativo = ativo,
            IdPlataforma = 1,
            IdCondicaoJogo = 1
        };
    }

    private void ConfigurarGravacaoComSucesso()
    {
        _repositorioJogo
            .Setup(r => r.BaixarEstoqueAsync(
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(true);

        _repositorioPedido
            .Setup(r => r.InserirAsync(
                It.IsAny<Pedido>()))
            .ReturnsAsync(123);

        _repositorioItemPedido
            .Setup(r => r.InserirAsync(
                It.IsAny<ItemPedido>()))
            .Returns(Task.CompletedTask);
    }

    private void VerificarQueNadaFoiGravado()
    {
        _repositorioJogo.Verify(
            r => r.BaixarEstoqueAsync(
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);

        _repositorioPedido.Verify(
            r => r.InserirAsync(
                It.IsAny<Pedido>()),
            Times.Never);

        _repositorioItemPedido.Verify(
            r => r.InserirAsync(
                It.IsAny<ItemPedido>()),
            Times.Never);
    }

    [Fact]
    public async Task CriarAsync_DadosValidos_CriaPedidoComSucesso()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(CriarJogo());

        ConfigurarGravacaoComSucesso();

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = 2
            }
        };

        // Act
        var resultado = await service.CriarAsync(itens);

        // Assert
        Assert.Equal(123, resultado.Id);
        Assert.Equal(42, resultado.IdUsuario);
        Assert.Equal(200m, resultado.Total);
        Assert.Equal(1, resultado.IdSituacaoPedido);
        Assert.Equal(DateTimeKind.Utc, resultado.DataPedido.Kind);

        _repositorioPedido.Verify(
            r => r.InserirAsync(
                It.Is<Pedido>(p =>
                    p.IdUsuario == 42 &&
                    p.Total == 200m &&
                    p.IdSituacaoPedido == 1)),
            Times.Once);

        _repositorioJogo.Verify(
            r => r.BaixarEstoqueAsync(1, 2),
            Times.Once);

        _repositorioItemPedido.Verify(
            r => r.InserirAsync(
                It.Is<ItemPedido>(item =>
                    item.IdJogo == 1 &&
                    item.Quantidade == 2 &&
                    item.ValorUnitario == 100m &&
                    item.IdPedido == 123)),
            Times.Once);
    }

    [Fact]
    public async Task CriarAsync_PrecoDoBanco_UsaPrecoCadastrado()
    {
        // Arrange
        var service = CriarService();

        var jogo = CriarJogo(preco: 150m);

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(jogo);

        ConfigurarGravacaoComSucesso();

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = 3
            }
        };

        // Act
        var resultado = await service.CriarAsync(itens);

        // Assert
        Assert.Equal(450m, resultado.Total);

        _repositorioItemPedido.Verify(
            r => r.InserirAsync(
                It.Is<ItemPedido>(item =>
                    item.ValorUnitario == 150m)),
            Times.Once);
    }

    [Fact]
    public async Task CriarAsync_JogoInexistente_LancaErroENaoGrava()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(999))
            .ReturnsAsync((Jogo?)null);

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 999,
                Quantidade = 1
            }
        };

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(itens));

        // Assert
        Assert.Equal(
            BusinessErrors.JOGO_INDISPONIVEL,
            excecao.Erro);

        VerificarQueNadaFoiGravado();
    }

    [Fact]
    public async Task CriarAsync_JogoInativo_LancaErroENaoGrava()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(CriarJogo(ativo: false));

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = 1
            }
        };

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(itens));

        // Assert
        Assert.Equal(
            BusinessErrors.JOGO_INDISPONIVEL,
            excecao.Erro);

        VerificarQueNadaFoiGravado();
    }

    [Fact]
    public async Task CriarAsync_EstoqueInsuficiente_LancaErroENaoGrava()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(CriarJogo(estoque: 2));

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = 3
            }
        };

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(itens));

        // Assert
        Assert.Equal(
            BusinessErrors.ESTOQUE_INSUFICIENTE,
            excecao.Erro);

        VerificarQueNadaFoiGravado();
    }

    [Fact]
    public async Task CriarAsync_EstoqueAlteradoDuranteCompra_LancaErro()
    {
        // Arrange
        var service = CriarService();

        _repositorioJogo
            .SetupSequence(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(CriarJogo(estoque: 5))
            .ReturnsAsync(CriarJogo(estoque: 0));

        _repositorioJogo
            .Setup(r => r.BaixarEstoqueAsync(1, 2))
            .ReturnsAsync(false);

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = 2
            }
        };

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(itens));

        // Assert
        Assert.Equal(
            BusinessErrors.ESTOQUE_INSUFICIENTE,
            excecao.Erro);

        _repositorioPedido.Verify(
            r => r.InserirAsync(It.IsAny<Pedido>()),
            Times.Never);

        _repositorioItemPedido.Verify(
            r => r.InserirAsync(It.IsAny<ItemPedido>()),
            Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ListaVazia_LancaExcecao()
    {
        // Arrange
        var service = CriarService();

        // Act
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CriarAsync(
                Array.Empty<ItemPedidoSolicitacaoServiceModel>()));

        // Assert
        VerificarQueNadaFoiGravado();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task CriarAsync_QuantidadeInvalida_LancaExcecao(
        int quantidade)
    {
        // Arrange
        var service = CriarService();

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = quantidade
            }
        };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CriarAsync(itens));

        // Assert
        VerificarQueNadaFoiGravado();

        _repositorioJogo.Verify(
            r => r.BuscarPorIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CriarAsync_JogoRepetido_LancaExcecao()
    {
        // Arrange
        var service = CriarService();

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = 1
            },
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = 1,
                Quantidade = 2
            }
        };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CriarAsync(itens));

        // Assert
        VerificarQueNadaFoiGravado();

        _repositorioJogo.Verify(
            r => r.BuscarPorIdAsync(It.IsAny<int>()),
            Times.Never);
    }
}
