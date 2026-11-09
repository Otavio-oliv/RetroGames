
using MySqlConnector;
using RetroGames.Repository.Config;
using RetroGames.Repository.Exceptions;

namespace RetroGames.Repository.Repositories;

public abstract class RepositoryBase
{
    protected readonly RepositoryConfig Config;

    protected RepositoryBase(RepositoryConfig config)
    {
        Config = config;
    }

    protected async Task<MySqlConnection> OpenConnectionAsync()
    {
        var connection = new MySqlConnection(Config.ConnectionString);

        await connection.OpenAsync();

        return connection;
    }

    protected MySqlCommand CreateCommand(
        MySqlConnection connection,
        string sql)
    {
        return new MySqlCommand(sql, connection);
    }

    protected void AddParameter(
        MySqlCommand command,
        string name,
        object? value)
    {
        command.Parameters.AddWithValue($"@{name}", value ?? DBNull.Value);
    }

    protected async Task<int> ExecuteWriteAsync(MySqlCommand command)
    {
        try
        {
            return await command.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            throw new RegistroDuplicadoException(
                "Já existe um registro com esse valor único.",
                ex);
        }
    }
}
