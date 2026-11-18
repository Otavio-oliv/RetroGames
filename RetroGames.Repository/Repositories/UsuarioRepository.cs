using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Repository.Repositories;

public class UsuarioRepository : RepositoryBase, IUsuarioRepository
{
    public UsuarioRepository(RepositoryConfig config)
        : base(config)
    {
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        const string sql = """
            SELECT
                co_seq_usuario,
                no_usuario,
                ds_email,
                ds_senha_hash,
                st_ativo,
                dt_cadastro,
                co_perfil
            FROM tb_usuario
            WHERE ds_email = @email;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "email", email);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapearUsuario(reader);
    }

    public async Task<Usuario?> BuscarPorIdAsync(int id)
    {
        const string sql = """
            SELECT
                co_seq_usuario,
                no_usuario,
                ds_email,
                ds_senha_hash,
                st_ativo,
                dt_cadastro,
                co_perfil
            FROM tb_usuario
            WHERE co_seq_usuario = @id;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapearUsuario(reader);
    }

    public async Task<bool> ExisteAdministradorAsync()
    {
        const string sql = """
            SELECT COUNT(*) > 0
            FROM tb_usuario
            WHERE co_perfil = 1;
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt32(resultado) == 1;
    }

    public async Task<int> InserirAsync(Usuario usuario)
    {
        const string sql = """
            INSERT INTO tb_usuario (
                no_usuario,
                ds_email,
                ds_senha_hash,
                st_ativo,
                co_perfil
            )
            VALUES (
                @nome,
                @email,
                @senhaHash,
                @ativo,
                @idPerfil
            );
            """;

        await using var connection = await OpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        AddParameter(command, "nome", usuario.Nome);
        AddParameter(command, "email", usuario.Email);
        AddParameter(command, "senhaHash", usuario.SenhaHash);
        AddParameter(command, "ativo", usuario.Ativo);
        AddParameter(command, "idPerfil", usuario.IdPerfil);

        await ExecuteWriteAsync(command);

        return Convert.ToInt32(command.LastInsertedId);
    }

    private static Usuario MapearUsuario(MySqlConnector.MySqlDataReader reader)
    {
        return new Usuario
        {
            Id = reader.GetInt32("co_seq_usuario"),
            Nome = reader.GetString("no_usuario"),
            Email = reader.GetString("ds_email"),
            SenhaHash = reader.GetString("ds_senha_hash"),
            Ativo = reader.GetBoolean("st_ativo"),
            DataCadastro = reader.GetDateTime("dt_cadastro"),
            IdPerfil = reader.GetInt32("co_perfil")
        };
    }
}
