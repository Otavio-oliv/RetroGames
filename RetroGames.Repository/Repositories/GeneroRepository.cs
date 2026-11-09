using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class GeneroRepository : RepositoryBase, IGeneroRepository
{
    public GeneroRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<List<Genero>> ListarTodosAsync()
    {
        const string sql = """
            SELECT
                co_seq_genero,
                no_genero
            FROM tb_genero
            ORDER BY no_genero;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync();

        var generos = new List<Genero>();

        while (await reader.ReadAsync())
        {
            generos.Add(new Genero
            {
                Id = reader.GetInt32("co_seq_genero"),
                Nome = reader.GetString("no_genero")
            });
        }

        return generos;
    }
}

