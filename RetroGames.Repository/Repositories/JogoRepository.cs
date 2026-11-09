
using System.Text;
using MySqlConnector;
using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class JogoRepository : RepositoryBase, IJogoRepository
{
    public JogoRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<(List<Jogo> Jogos, int Total)> ListarAsync(
        string? busca = null,
        int? idPlataforma = null,
        IReadOnlyCollection<int>? idsJogos = null,
        int pagina = 1,
        int tamanhoPagina = 10,
        bool incluirInativos = false)
    {
        // Uma lista vazia significa que não existem jogos correspondentes.
        if (idsJogos is { Count: 0 })
            return (new List<Jogo>(), 0);

        await using var connection = await OpenConnectionAsync();

        // Consulta a quantidade total de registros.
        await using var commandContagem = CreateCommand(
            connection,
            "");

        var filtrosContagem = MontarFiltros(
            commandContagem,
            busca,
            idPlataforma,
            idsJogos,
            incluirInativos);

        commandContagem.CommandText = $"""
            SELECT COUNT(*)
            FROM tb_jogo
            {filtrosContagem};
            """;

        var resultadoTotal = await commandContagem.ExecuteScalarAsync();
        var total = Convert.ToInt32(resultadoTotal);

        // Consulta os jogos da página solicitada.
        await using var commandListagem = CreateCommand(
            connection,
            "");

        var filtrosListagem = MontarFiltros(
            commandListagem,
            busca,
            idPlataforma,
            idsJogos,
            incluirInativos);

        commandListagem.CommandText = $"""
            SELECT
                co_seq_jogo,
                no_jogo,
                ds_jogo,
                nu_ano_lancamento,
                vl_preco,
                qt_estoque,
                st_ativo,
                dt_cadastro,
                co_plataforma,
                co_condicao_jogo
            FROM tb_jogo
            {filtrosListagem}
            ORDER BY no_jogo
            LIMIT @tamanhoPagina OFFSET @offset;
            """;

        AddParameter(commandListagem, "tamanhoPagina", tamanhoPagina);
        AddParameter(
            commandListagem,
            "offset",
            (pagina - 1) * tamanhoPagina);

        await using var reader = await commandListagem.ExecuteReaderAsync();

        var jogos = new List<Jogo>();

        while (await reader.ReadAsync())
        {
            jogos.Add(MapearJogo(reader));
        }

        return (jogos, total);
    }

    public async Task<Jogo?> BuscarPorIdAsync(int id)
    {
        const string sql = """
            SELECT
                co_seq_jogo,
                no_jogo,
                ds_jogo,
                nu_ano_lancamento,
                vl_preco,
                qt_estoque,
                st_ativo,
                dt_cadastro,
                co_plataforma,
                co_condicao_jogo
            FROM tb_jogo
            WHERE co_seq_jogo = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapearJogo(reader);
    }

    public async Task<int> InserirAsync(Jogo jogo)
    {
        const string sql = """
            INSERT INTO tb_jogo (
                no_jogo,
                ds_jogo,
                nu_ano_lancamento,
                vl_preco,
                qt_estoque,
                st_ativo,
                co_plataforma,
                co_condicao_jogo
            )
            VALUES (
                @nome,
                @descricao,
                @anoLancamento,
                @preco,
                @estoque,
                @ativo,
                @idPlataforma,
                @idCondicaoJogo
            );
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AdicionarParametrosJogo(command, jogo);

        await ExecuteWriteAsync(command);

        return Convert.ToInt32(command.LastInsertedId);
    }

    public async Task AlterarAsync(Jogo jogo)
    {
        const string sql = """
            UPDATE tb_jogo
            SET
                no_jogo = @nome,
                ds_jogo = @descricao,
                nu_ano_lancamento = @anoLancamento,
                vl_preco = @preco,
                qt_estoque = @estoque,
                st_ativo = @ativo,
                co_plataforma = @idPlataforma,
                co_condicao_jogo = @idCondicaoJogo
            WHERE co_seq_jogo = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", jogo.Id);
        AdicionarParametrosJogo(command, jogo);

        await ExecuteWriteAsync(command);
    }

    public async Task DesativarAsync(int id)
    {
        const string sql = """
            UPDATE tb_jogo
            SET st_ativo = 0
            WHERE co_seq_jogo = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", id);

        await ExecuteWriteAsync(command);
    }

    private string MontarFiltros(
        MySqlCommand command,
        string? busca,
        int? idPlataforma,
        IReadOnlyCollection<int>? idsJogos,
        bool incluirInativos)
    {
        var filtros = new StringBuilder("WHERE 1 = 1");

        if (!incluirInativos)
            filtros.Append(" AND st_ativo = 1");

        if (!string.IsNullOrWhiteSpace(busca))
        {
            filtros.Append(
                " AND no_jogo LIKE CONCAT('%', @busca, '%')");

            AddParameter(command, "busca", busca);
        }

        if (idPlataforma.HasValue)
        {
            filtros.Append(" AND co_plataforma = @idPlataforma");

            AddParameter(
                command,
                "idPlataforma",
                idPlataforma.Value);
        }

        if (idsJogos is not null)
        {
            var parametros = new List<string>();
            var indice = 0;

            foreach (var idJogo in idsJogos)
            {
                var nomeParametro = $"idJogo{indice}";

                parametros.Add($"@{nomeParametro}");
                AddParameter(command, nomeParametro, idJogo);

                indice++;
            }

            filtros.Append(
                $" AND co_seq_jogo IN ({string.Join(", ", parametros)})");
        }

        return filtros.ToString();
    }

    private void AdicionarParametrosJogo(
        MySqlCommand command,
        Jogo jogo)
    {
        AddParameter(command, "nome", jogo.Nome);
        AddParameter(command, "descricao", jogo.Descricao);
        AddParameter(command, "anoLancamento", jogo.AnoLancamento);
        AddParameter(command, "preco", jogo.Preco);
        AddParameter(command, "estoque", jogo.Estoque);
        AddParameter(command, "ativo", jogo.Ativo);
        AddParameter(command, "idPlataforma", jogo.IdPlataforma);
        AddParameter(command, "idCondicaoJogo", jogo.IdCondicaoJogo);
    }

    private Jogo MapearJogo(MySqlDataReader reader)
    {
        return new Jogo
        {
            Id = reader.GetInt32("co_seq_jogo"),
            Nome = reader.GetString("no_jogo"),
            Descricao = reader.IsDBNull(
                reader.GetOrdinal("ds_jogo"))
                    ? null
                    : reader.GetString("ds_jogo"),
            AnoLancamento = reader.GetInt32("nu_ano_lancamento"),
            Preco = reader.GetDecimal("vl_preco"),
            Estoque = reader.GetInt32("qt_estoque"),
            Ativo = reader.GetBoolean("st_ativo"),
            DataCadastro = reader.GetDateTime("dt_cadastro"),
            IdPlataforma = reader.GetInt32("co_plataforma"),
            IdCondicaoJogo = reader.GetInt32("co_condicao_jogo")
        };
    }
}
