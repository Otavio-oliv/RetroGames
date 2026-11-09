
using MySqlConnector;
using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class JogoGeneroRepository : RepositoryBase, IJogoGeneroRepository
{
    public JogoGeneroRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<List<JogoGenero>> ListarTodosAsync()
    {
        const string sql = """
            SELECT
                co_jogo,
                co_genero
            FROM tb_jogo_genero
            ORDER BY co_jogo, co_genero;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync();

        var vinculos = new List<JogoGenero>();

        while (await reader.ReadAsync())
        {
            vinculos.Add(MapearVinculo(reader));
        }

        return vinculos;
    }

    public async Task<List<JogoGenero>> ListarPorJogosAsync(
        IReadOnlyCollection<int> idsJogos)
    {
        if (idsJogos.Count == 0)
            return new List<JogoGenero>();

        var parametros = new List<string>();
        var indice = 0;

        foreach (var idJogo in idsJogos)
        {
            parametros.Add($"@idJogo{indice}");
            indice++;
        }

        var sql = $"""
            SELECT
                co_jogo,
                co_genero
            FROM tb_jogo_genero
            WHERE co_jogo IN ({string.Join(", ", parametros)})
            ORDER BY co_jogo, co_genero;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        indice = 0;

        foreach (var idJogo in idsJogos)
        {
            AddParameter(command, $"idJogo{indice}", idJogo);
            indice++;
        }

        await using var reader = await command.ExecuteReaderAsync();

        var vinculos = new List<JogoGenero>();

        while (await reader.ReadAsync())
        {
            vinculos.Add(MapearVinculo(reader));
        }

        return vinculos;
    }

    public async Task InserirAsync(
        IReadOnlyCollection<JogoGenero> vinculos)
    {
        if (vinculos.Count == 0)
            return;

        await using var connection = await OpenConnectionAsync();

        foreach (var vinculo in vinculos)
        {
            const string sql = """
                INSERT INTO tb_jogo_genero (
                    co_jogo,
                    co_genero
                )
                VALUES (
                    @idJogo,
                    @idGenero
                );
                """;

            await using var command = CreateCommand(connection, sql);

            AddParameter(command, "idJogo", vinculo.IdJogo);
            AddParameter(command, "idGenero", vinculo.IdGenero);

            await ExecuteWriteAsync(command);
        }
    }

    public async Task RemoverPorJogoAsync(int idJogo)
    {
        const string sql = """
            DELETE FROM tb_jogo_genero
            WHERE co_jogo = @idJogo;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "idJogo", idJogo);

        await ExecuteWriteAsync(command);
    }

    private JogoGenero MapearVinculo(MySqlDataReader reader)
    {
        return new JogoGenero
        {
            IdJogo = reader.GetInt32("co_jogo"),
            IdGenero = reader.GetInt32("co_genero")
        };
    }
}
