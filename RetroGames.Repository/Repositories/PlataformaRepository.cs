using MySqlConnector;
using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Exceptions;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class PlataformaRepository : RepositoryBase, IPlataformaRepository
{
    public PlataformaRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<List<Plataforma>> ListarAsync()
    {
        const string sql = """
            SELECT
                co_seq_plataforma,
                no_plataforma,
                no_fabricante,
                nu_ano_lancamento
            FROM tb_plataforma
            ORDER BY no_plataforma;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync();

        var plataformas = new List<Plataforma>();

        while (await reader.ReadAsync())
        {
            plataformas.Add(new Plataforma
            {
                Id = reader.GetInt32("co_seq_plataforma"),
                Nome = reader.GetString("no_plataforma"),
                Fabricante = reader.GetString("no_fabricante"),
                AnoLancamento = reader.GetInt32("nu_ano_lancamento")
            });
        }

        return plataformas;
    }

    public async Task<Plataforma?> BuscarPorIdAsync(int id)
    {
        const string sql = """
            SELECT
                co_seq_plataforma,
                no_plataforma,
                no_fabricante,
                nu_ano_lancamento
            FROM tb_plataforma
            WHERE co_seq_plataforma = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Plataforma
        {
            Id = reader.GetInt32("co_seq_plataforma"),
            Nome = reader.GetString("no_plataforma"),
            Fabricante = reader.GetString("no_fabricante"),
            AnoLancamento = reader.GetInt32("nu_ano_lancamento")
        };
    }

    public async Task<bool> ExisteComNomeAsync(
        string nome,
        int? idIgnorado = null)
    {
        const string sql = """
            SELECT COUNT(*) > 0
            FROM tb_plataforma
            WHERE no_plataforma = @nome
              AND (
                  @idIgnorado IS NULL
                  OR co_seq_plataforma <> @idIgnorado
              );
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "nome", nome);
        AddParameter(command, "idIgnorado", idIgnorado);

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt32(resultado) == 1;
    }

    public async Task<bool> PossuiJogosAsync(int idPlataforma)
    {
        const string sql = """
            SELECT COUNT(*) > 0
            FROM tb_jogo
            WHERE co_plataforma = @idPlataforma;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "idPlataforma", idPlataforma);

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt32(resultado) == 1;
    }

    public async Task<int> InserirAsync(Plataforma plataforma)
    {
        const string sql = """
            INSERT INTO tb_plataforma (
                no_plataforma,
                no_fabricante,
                nu_ano_lancamento
            )
            VALUES (
                @nome,
                @fabricante,
                @anoLancamento
            );
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "nome", plataforma.Nome);
        AddParameter(command, "fabricante", plataforma.Fabricante);
        AddParameter(command, "anoLancamento", plataforma.AnoLancamento);

        await ExecuteWriteAsync(command);

        return Convert.ToInt32(command.LastInsertedId);
    }

    public async Task AlterarAsync(Plataforma plataforma)
    {
        const string sql = """
            UPDATE tb_plataforma
            SET
                no_plataforma = @nome,
                no_fabricante = @fabricante,
                nu_ano_lancamento = @anoLancamento
            WHERE co_seq_plataforma = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", plataforma.Id);
        AddParameter(command, "nome", plataforma.Nome);
        AddParameter(command, "fabricante", plataforma.Fabricante);
        AddParameter(command, "anoLancamento", plataforma.AnoLancamento);

        await ExecuteWriteAsync(command);
    }

    public async Task ExcluirAsync(int id)
    {
        const string sql = """
            DELETE FROM tb_plataforma
            WHERE co_seq_plataforma = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", id);

        await ExecuteWriteAsync(command);
    }
}
