using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class CondicaoJogoRepository : RepositoryBase, ICondicaoJogoRepository
{
    public CondicaoJogoRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<List<CondicaoJogo>> ListarTodosAsync()
    {
        const string sql = """
            SELECT
                co_condicao_jogo,
                ds_condicao_jogo
            FROM tb_condicao_jogo
            ORDER BY ds_condicao_jogo;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync();

        var condicoes = new List<CondicaoJogo>();

        while (await reader.ReadAsync())
        {
            condicoes.Add(new CondicaoJogo
            {
                Id = reader.GetInt32("co_condicao_jogo"),
                Descricao = reader.GetString("ds_condicao_jogo")
            });
        }

        return condicoes;
    }
}
