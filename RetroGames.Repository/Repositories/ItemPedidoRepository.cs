using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class ItemPedidoRepository
    : RepositoryBase, IItemPedidoRepository
{
    public ItemPedidoRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task InserirAsync(ItemPedido item)
    {
        const string sql = """
            INSERT INTO tb_item_pedido (
                qt_item,
                vl_unitario,
                co_pedido,
                co_jogo
            )
            VALUES (
                @quantidade,
                @valorUnitario,
                @idPedido,
                @idJogo
            );
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "quantidade", item.Quantidade);
        AddParameter(command, "valorUnitario", item.ValorUnitario);
        AddParameter(command, "idPedido", item.IdPedido);
        AddParameter(command, "idJogo", item.IdJogo);

        await ExecuteWriteAsync(command);
    }

    public async Task<List<ItemPedido>> ListarPorPedidoAsync(int idPedido)
    {
        const string sql = """
            SELECT
                co_seq_item_pedido,
                qt_item,
                vl_unitario,
                co_pedido,
                co_jogo
            FROM tb_item_pedido
            WHERE co_pedido = @idPedido
            ORDER BY co_seq_item_pedido;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "idPedido", idPedido);

        await using var reader = await command.ExecuteReaderAsync();

        var itens = new List<ItemPedido>();

        while (await reader.ReadAsync())
        {
            itens.Add(new ItemPedido
            {
                Id = reader.GetInt32("co_seq_item_pedido"),
                Quantidade = reader.GetInt32("qt_item"),
                ValorUnitario = reader.GetDecimal("vl_unitario"),
                IdPedido = reader.GetInt32("co_pedido"),
                IdJogo = reader.GetInt32("co_jogo")
            });
        }

        return itens;
    }
}
