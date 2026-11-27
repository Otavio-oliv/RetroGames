
using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class SituacaoPedidoRepository
    : RepositoryBase, ISituacaoPedidoRepository
{
    public SituacaoPedidoRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<SituacaoPedido?> BuscarPorIdAsync(int id)
    {
        const string sql = """
            SELECT
                co_situacao_pedido,
                ds_situacao_pedido
            FROM tb_situacao_pedido
            WHERE co_situacao_pedido = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new SituacaoPedido
        {
            Id = reader.GetInt32("co_situacao_pedido"),
            Descricao = reader.GetString("ds_situacao_pedido")
        };
    }

    public async Task<List<SituacaoPedido>> ListarTodosAsync()
    {
        const string sql = """
            SELECT
                co_situacao_pedido,
                ds_situacao_pedido
            FROM tb_situacao_pedido
            ORDER BY co_situacao_pedido;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync();

        var situacoes = new List<SituacaoPedido>();

        while (await reader.ReadAsync())
        {
            situacoes.Add(new SituacaoPedido
            {
                Id = reader.GetInt32("co_situacao_pedido"),
                Descricao = reader.GetString("ds_situacao_pedido")
            });
        }

        return situacoes;
    }
}
