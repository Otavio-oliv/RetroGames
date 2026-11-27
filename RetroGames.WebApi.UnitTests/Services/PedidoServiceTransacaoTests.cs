
using System.Transactions;
using Moq;
using MySqlConnector;
using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;
using RetroGames.Repository.Repositories;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

public class PedidoServiceTransacaoTests
{
    private const int IdUsuarioTeste = 2;

    [Fact]
    public async Task CriarAsync_FalhaAoInserirItem_DeveDesfazerPedidoEEstoque()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "A variável ConnectionStrings__Default não foi carregada.");

        var config = new RepositoryConfig(connectionString!);

        // Repositórios reais: as operações serão executadas no MariaDB.
        var repositorioJogo = new JogoRepository(config);
        var repositorioPedido = new PedidoRepository(config);

        // Somente o repositório de itens será simulado.
        var repositorioItemPedido =
            new Mock<IItemPedidoRepository>();

        repositorioItemPedido
            .Setup(r => r.InserirAsync(It.IsAny<ItemPedido>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Falha intencional para testar o rollback."));

        var repositorios = new Mock<IRepositorySet>();

        repositorios
            .SetupGet(r => r.Jogo)
            .Returns(repositorioJogo);

        repositorios
            .SetupGet(r => r.Pedido)
            .Returns(repositorioPedido);

        repositorios
            .SetupGet(r => r.ItemPedido)
            .Returns(repositorioItemPedido.Object);

        var usuarioAtual = new Mock<IUsuarioAtualService>();

        usuarioAtual
            .SetupGet(u => u.EstaAutenticado)
            .Returns(true);

        usuarioAtual
            .SetupGet(u => u.IdUsuario)
            .Returns(IdUsuarioTeste);

        var service = new PedidoService(
            repositorios.Object,
            usuarioAtual.Object);

        // Escolhe um jogo ativo com estoque disponível.
        var jogoSelecionado = await BuscarJogoDisponivelAsync(
            connectionString!);

        var estadoAntes = await ConsultarEstadoAsync(
            connectionString!,
            jogoSelecionado.Id);

        // Confirma que existe estoque para a operação.
        Assert.True(estadoAntes.Estoque > 0);

        var itens = new[]
        {
            new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = jogoSelecionado.Id,
                Quantidade = 1
            }
        };

        // A falha será provocada depois da baixa do estoque
        // e da tentativa de inserção do pedido.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CriarAsync(itens));

        // Consulta novamente os dados diretamente no MariaDB.
        var estadoDepois = await ConsultarEstadoAsync(
            connectionString!,
            jogoSelecionado.Id);

        // O estoque precisa voltar ao valor original.
        Assert.Equal(
            estadoAntes.Estoque,
            estadoDepois.Estoque);

        // O pedido também precisa ter sido desfeito.
        Assert.Equal(
            estadoAntes.QuantidadePedidos,
            estadoDepois.QuantidadePedidos);

        // Confirma que a falha foi realmente provocada
        // pelo repositório de itens.
        repositorioItemPedido.Verify(
            r => r.InserirAsync(It.IsAny<ItemPedido>()),
            Times.Once);
    }

    private static async Task<(int Id, int Estoque)>
        BuscarJogoDisponivelAsync(string connectionString)
    {
        const string sql = """
            SELECT co_seq_jogo, qt_estoque
            FROM tb_jogo
            WHERE st_ativo = 1
              AND qt_estoque > 0
            ORDER BY co_seq_jogo
            LIMIT 1;
            """;

        await using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        Assert.True(
            await reader.ReadAsync(),
            "Não existe jogo ativo com estoque disponível para o teste.");

        return (
            reader.GetInt32("co_seq_jogo"),
            reader.GetInt32("qt_estoque")
        );
    }

    private static async Task<(int Estoque, long QuantidadePedidos)>
        ConsultarEstadoAsync(
            string connectionString,
            int idJogo)
    {
        const string sql = """
            SELECT
                j.qt_estoque,
                (
                    SELECT COUNT(*)
                    FROM tb_pedido p
                    WHERE p.co_usuario = @idUsuario
                ) AS quantidade_pedidos
            FROM tb_jogo j
            WHERE j.co_seq_jogo = @idJogo;
            """;

        await using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@idJogo", idJogo);
        command.Parameters.AddWithValue(
            "@idUsuario",
            IdUsuarioTeste);

        await using var reader =
            await command.ExecuteReaderAsync();

        Assert.True(
            await reader.ReadAsync(),
            "Não foi possível consultar o estado do jogo.");

        return (
            reader.GetInt32("qt_estoque"),
            reader.GetInt64("quantidade_pedidos")
        );
    }
}
