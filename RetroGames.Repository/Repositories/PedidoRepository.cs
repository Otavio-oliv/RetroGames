using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class PedidoRepository : RepositoryBase, IPedidoRepository
{
    public PedidoRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<int> InserirAsync(Pedido pedido)
    {
        const string sql = """
            INSERT INTO tb_pedido (
                dt_pedido,
                vl_total,
                co_usuario,
                co_situacao_pedido
            )
            VALUES (
                @dataPedido,
                @total,
                @idUsuario,
                @idSituacaoPedido
            );
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "dataPedido", pedido.DataPedido);
        AddParameter(command, "total", pedido.Total);
        AddParameter(command, "idUsuario", pedido.IdUsuario);
        AddParameter(
            command,
            "idSituacaoPedido",
            pedido.IdSituacaoPedido);

        await ExecuteWriteAsync(command);

        return Convert.ToInt32(command.LastInsertedId);
    }

    public async Task<List<Pedido>> ListarPorUsuarioAsync(int idUsuario)
    {
        const string sql = """
            SELECT
                co_seq_pedido,
                dt_pedido,
                vl_total,
                co_usuario,
                co_situacao_pedido
            FROM tb_pedido
            WHERE co_usuario = @idUsuario
            ORDER BY dt_pedido DESC, co_seq_pedido DESC;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();

        var pedidos = new List<Pedido>();

        while (await reader.ReadAsync())
        {
            pedidos.Add(new Pedido
            {
                Id = reader.GetInt32("co_seq_pedido"),
                DataPedido = reader.GetDateTime("dt_pedido"),
                Total = reader.GetDecimal("vl_total"),
                IdUsuario = reader.GetInt32("co_usuario"),
                IdSituacaoPedido = reader.GetInt32("co_situacao_pedido")
            });
        }

        return pedidos;
    }
}
