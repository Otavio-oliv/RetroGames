namespace RetroGames.Repository.Config;

public class RepositoryConfig
{
    public string ConnectionString { get; }

    public RepositoryConfig(string connectionString)
    {
        ConnectionString = connectionString;
    }
}
