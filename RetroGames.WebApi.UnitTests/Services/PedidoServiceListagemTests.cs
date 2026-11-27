using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

public class PedidoServiceListagemTests
{
    private readonly Mock<IRepositorySet> _repositorios;
    private readonly Mock<IPedidoRepository> _repositorioPedido;
    private readonly Mock<IItemPedidoRepository> _repositorioItemPedido;
    private readonly Mock<IJogoRepository> _repositorioJogo;
    private readonly Mock<ISituacaoPedidoRepository> _repositorioSituacao;
    private readonly Mock<IUsuarioAtualService> _usuarioAtual;

    public PedidoServiceListagemTests()
    {
        _repositorios = new Mock<IRepositorySet>();
        _repositorioPedido = new Mock<IPedidoRepository>();
        _repositorioItemPedido = new Mock<IItemPedidoRepository>();
        _repositorioJogo = new Mock<IJogoRepository>();
        _repositorioSituacao = new Mock<ISituacaoPedidoRepository>();
        _usuarioAtual = new Mock<IUsuarioAtualService>();

        _repositorios
            .SetupGet(r => r.Pedido)
            .Returns(_repositorioPedido.Object);

        _repositorios
            .SetupGet(r => r.ItemPedido)
            .Returns(_repositorioItemPedido.Object);

        _repositorios
            .SetupGet(r => r.Jogo)
            .Returns(_repositorioJogo.Object);

        _repositorios
            .SetupGet(r => r.SituacaoPedido)
            .Returns(_repositorioSituacao.Object);

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

    [Fact]
    public async Task ListarMeusAsync_UsuarioAutenticado_ConsultaPedidosDoUsuarioAtual()
    {
        // Arrange
        var service = CriarService();

        _repositorioPedido
            .Setup(r => r.ListarPorUsuarioAsync(42))
            .ReturnsAsync(new List<Pedido>());

        // Act
        var resultado = await service.ListarMeusAsync();

        // Assert
        Assert.Empty(resultado);

        _repositorioPedido.Verify(
            r => r.ListarPorUsuarioAsync(42),
            Times.Once);

        _repositorioPedido.Verify(
            r => r.ListarPorUsuarioAsync(It.Is<int>(id => id != 42)),
            Times.Never);
    }

    [Fact]
    public async Task ListarMeusAsync_PedidosComItens_RetornaDetalhesCompletos()
    {
        // Arrange
        var service = CriarService();

        var pedidoMaisRecente = new Pedido
        {
            Id = 20,
            DataPedido = new DateTime(2026, 11, 20, 15, 0, 0),
            Total = 300m,
            IdUsuario = 42,
            IdSituacaoPedido = 2
        };

        var pedidoMaisAntigo = new Pedido
        {
            Id = 10,
            DataPedido = new DateTime(2026, 11, 10, 15, 0, 0),
            Total = 200m,
            IdUsuario = 42,
            IdSituacaoPedido = 1
        };

        // A ordem recebida do repositório representa a ordem
        // definida pela consulta SQL: mais recente primeiro.
        _repositorioPedido
            .Setup(r => r.ListarPorUsuarioAsync(42))
            .ReturnsAsync(new List<Pedido>
            {
                pedidoMaisRecente,
                pedidoMaisAntigo
            });

        _repositorioSituacao
            .Setup(r => r.BuscarPorIdAsync(2))
            .ReturnsAsync(new SituacaoPedido
            {
                Id = 2,
                Descricao = "Pago"
            });

        _repositorioSituacao
            .Setup(r => r.BuscarPorIdAsync(1))
            .ReturnsAsync(new SituacaoPedido
            {
                Id = 1,
                Descricao = "Pendente"
            });

        _repositorioItemPedido
            .Setup(r => r.ListarPorPedidoAsync(20))
            .ReturnsAsync(new List<ItemPedido>
            {
                new ItemPedido
                {
                    Id = 200,
                    IdPedido = 20,
                    IdJogo = 5,
                    Quantidade = 2,
                    ValorUnitario = 100m
                },
                new ItemPedido
                {
                    Id = 201,
                    IdPedido = 20,
                    IdJogo = 6,
                    Quantidade = 1,
                    ValorUnitario = 100m
                }
            });

        _repositorioItemPedido
            .Setup(r => r.ListarPorPedidoAsync(10))
            .ReturnsAsync(new List<ItemPedido>
            {
                new ItemPedido
                {
                    Id = 100,
                    IdPedido = 10,
                    IdJogo = 7,
                    Quantidade = 1,
                    ValorUnitario = 200m
                }
            });

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(5))
            .ReturnsAsync(new Jogo
            {
                Id = 5,
                Nome = "Super Mario Bros",
                Preco = 150m
            });

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(6))
            .ReturnsAsync(new Jogo
            {
                Id = 6,
                Nome = "The Legend of Zelda",
                Preco = 120m
            });

        _repositorioJogo
            .Setup(r => r.BuscarPorIdAsync(7))
            .ReturnsAsync(new Jogo
            {
                Id = 7,
                Nome = "Metroid",
                Preco = 250m
            });

        // Act
        var resultado = await service.ListarMeusAsync();

        // Assert
        Assert.Equal(2, resultado.Count);

        // Verifica a ordem dos pedidos.
        Assert.Equal(20, resultado[0].Pedido.Id);
        Assert.Equal(10, resultado[1].Pedido.Id);

        // Verifica os dados do pedido mais recente.
        Assert.Equal(300m, resultado[0].Pedido.Total);
        Assert.Equal(2, resultado[0].Pedido.IdSituacaoPedido);
        Assert.Equal("Pago", resultado[0].DescricaoSituacao);

        // Verifica os itens do pedido mais recente.
        Assert.Equal(2, resultado[0].Itens.Count);

        Assert.Equal(
            "Super Mario Bros",
            resultado[0].Itens[0].NomeJogo);

        Assert.Equal(
            2,
            resultado[0].Itens[0].Item.Quantidade);

        Assert.Equal(
            100m,
            resultado[0].Itens[0].Item.ValorUnitario);

        Assert.Equal(
            "The Legend of Zelda",
            resultado[0].Itens[1].NomeJogo);

        Assert.Equal(
            1,
            resultado[0].Itens[1].Item.Quantidade);

        Assert.Equal(
            100m,
            resultado[0].Itens[1].Item.ValorUnitario);

        // Verifica os dados do pedido mais antigo.
        Assert.Equal("Pendente", resultado[1].DescricaoSituacao);
        Assert.Single(resultado[1].Itens);
        Assert.Equal("Metroid", resultado[1].Itens[0].NomeJogo);

        // O valor unitário deve ser o valor histórico do pedido,
        // mesmo que o preço atual do jogo seja diferente.
        Assert.Equal(
            200m,
            resultado[1].Itens[0].Item.ValorUnitario);

        // Verifica que os pedidos foram buscados para o usuário atual.
        _repositorioPedido.Verify(
            r => r.ListarPorUsuarioAsync(42),
            Times.Once);

        // Verifica que os detalhes foram consultados.
        _repositorioSituacao.Verify(
            r => r.BuscarPorIdAsync(2),
            Times.Once);

        _repositorioSituacao.Verify(
            r => r.BuscarPorIdAsync(1),
            Times.Once);

        _repositorioItemPedido.Verify(
            r => r.ListarPorPedidoAsync(20),
            Times.Once);

        _repositorioItemPedido.Verify(
            r => r.ListarPorPedidoAsync(10),
            Times.Once);
    }

    [Fact]
    public async Task ListarMeusAsync_SemPedidos_RetornaListaVazia()
    {
        // Arrange
        var service = CriarService();

        _repositorioPedido
            .Setup(r => r.ListarPorUsuarioAsync(42))
            .ReturnsAsync(new List<Pedido>());

        // Act
        var resultado = await service.ListarMeusAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);

        _repositorioItemPedido.Verify(
            r => r.ListarPorPedidoAsync(It.IsAny<int>()),
            Times.Never);

        _repositorioSituacao.Verify(
            r => r.BuscarPorIdAsync(It.IsAny<int>()),
            Times.Never);

        _repositorioJogo.Verify(
            r => r.BuscarPorIdAsync(It.IsAny<int>()),
            Times.Never);
    }
}
