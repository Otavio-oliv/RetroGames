using RetroGames.Repository.Config;
using RetroGames.Repository.Repositories;
using RetroGames.Model.Model;

namespace RetroGames.WebApi.UnitTests;

public class UnitTest1
{
    [Fact]
    public async Task DeveListarPlataformasDoBanco()
    {
        // Localiza o arquivo .env na raiz do projeto.
        var diretorio = Directory.GetCurrentDirectory();

        while (!File.Exists(Path.Combine(diretorio, ".env")))
        {
            diretorio = Directory.GetParent(diretorio)?.FullName
                ?? throw new FileNotFoundException(
                    "Arquivo .env não encontrado.");
        }

        // Lê a connection string sem exibir a senha.
        var linha = File.ReadAllLines(
                Path.Combine(diretorio, ".env"))
            .First(l => l.StartsWith("ConnectionStrings__Default="));

        var connectionString = linha
            .Split('=', 2)[1]
            .Trim()
            .Trim('"');

        // Configura e instancia o repository.
        var config = new RepositoryConfig(connectionString);
        var repository = new PlataformaRepository(config);

        // Consulta as plataformas.
        var plataformas = await repository.ListarAsync();

        // Verifica o resultado.
        Assert.NotNull(plataformas);
        Assert.NotEmpty(plataformas);

        Assert.Contains(plataformas, p => p.Nome == "NES");
    } [Fact]
public async Task DeveBuscarPlataformaPorId()
{
    var diretorio = Directory.GetCurrentDirectory();

    while (!File.Exists(Path.Combine(diretorio, ".env")))
    {
        diretorio = Directory.GetParent(diretorio)?.FullName
            ?? throw new FileNotFoundException(
                "Arquivo .env não encontrado.");
    }

    var linha = File.ReadAllLines(
            Path.Combine(diretorio, ".env"))
        .First(l => l.StartsWith("ConnectionStrings__Default="));

    var connectionString = linha
        .Split('=', 2)[1]
        .Trim()
        .Trim('"');

    var config = new RepositoryConfig(connectionString);
    var repository = new PlataformaRepository(config);

    var plataforma = await repository.BuscarPorIdAsync(1);

    Assert.NotNull(plataforma);
    Assert.Equal("NES", plataforma.Nome);
    Assert.Equal("Nintendo", plataforma.Fabricante);
} [Fact]
public async Task DeveVerificarSeNomeDaPlataformaExiste()
{
    var diretorio = Directory.GetCurrentDirectory();

    while (!File.Exists(Path.Combine(diretorio, ".env")))
    {
        diretorio = Directory.GetParent(diretorio)?.FullName
            ?? throw new FileNotFoundException(
                "Arquivo .env não encontrado.");
    }

    var linha = File.ReadAllLines(
            Path.Combine(diretorio, ".env"))
        .First(l => l.StartsWith("ConnectionStrings__Default="));

    var connectionString = linha
        .Split('=', 2)[1]
        .Trim()
        .Trim('"');

    var config = new RepositoryConfig(connectionString);
    var repository = new PlataformaRepository(config);

    var nomeExiste = await repository.ExisteComNomeAsync("NES");
    var nomeNaoExiste = await repository.ExisteComNomeAsync("Plataforma Inexistente");
    var nomeIgnorandoProprioId = await repository.ExisteComNomeAsync("NES", 1);

    Assert.True(nomeExiste);
    Assert.False(nomeNaoExiste);
    Assert.False(nomeIgnorandoProprioId);
} [Fact]
public async Task DeveVerificarSePlataformaPossuiJogos()
{
    var diretorio = Directory.GetCurrentDirectory();

    while (!File.Exists(Path.Combine(diretorio, ".env")))
    {
        diretorio = Directory.GetParent(diretorio)?.FullName
            ?? throw new FileNotFoundException(
                "Arquivo .env não encontrado.");
    }

    var linha = File.ReadAllLines(
            Path.Combine(diretorio, ".env"))
        .First(l => l.StartsWith("ConnectionStrings__Default="));

    var connectionString = linha
        .Split('=', 2)[1]
        .Trim()
        .Trim('"');

    var config = new RepositoryConfig(connectionString);
    var repository = new PlataformaRepository(config);

    var possuiJogos = await repository.PossuiJogosAsync(1);
    var naoPossuiJogos = await repository.PossuiJogosAsync(9999);

    Assert.True(possuiJogos);
    Assert.False(naoPossuiJogos);
}
[Fact]
public async Task DeveInserirAlterarEExcluirPlataforma()
{
    var diretorio = Directory.GetCurrentDirectory();

    while (!File.Exists(Path.Combine(diretorio, ".env")))
    {
        diretorio = Directory.GetParent(diretorio)?.FullName
            ?? throw new FileNotFoundException(
                "Arquivo .env não encontrado.");
    }

    var linha = File.ReadAllLines(
            Path.Combine(diretorio, ".env"))
        .First(l => l.StartsWith("ConnectionStrings__Default="));

    var connectionString = linha
        .Split('=', 2)[1]
        .Trim()
        .Trim('"');

    var config = new RepositoryConfig(connectionString);
    var repository = new PlataformaRepository(config);

    int id = 0;

    try
    {
        // Insere uma plataforma temporária.
        var plataforma = new Plataforma
        {
            Nome = $"Teste {Guid.NewGuid()}",
            Fabricante = "Fabricante Teste",
            AnoLancamento = 2026
        };

        id = await repository.InserirAsync(plataforma);

        Assert.True(id > 0);

        // Confirma que foi cadastrada.
        var cadastrada = await repository.BuscarPorIdAsync(id);

        Assert.NotNull(cadastrada);
        Assert.Equal(plataforma.Nome, cadastrada.Nome);

        // Altera os dados.
        cadastrada.Nome = $"Atualizada {Guid.NewGuid()}";
        cadastrada.Fabricante = "Fabricante Atualizado";
        cadastrada.AnoLancamento = 2025;

        await repository.AlterarAsync(cadastrada);

        // Confirma a alteração.
        var atualizada = await repository.BuscarPorIdAsync(id);

        Assert.NotNull(atualizada);
        Assert.Equal(cadastrada.Nome, atualizada.Nome);
        Assert.Equal("Fabricante Atualizado", atualizada.Fabricante);
        Assert.Equal(2025, atualizada.AnoLancamento);

        // Exclui a plataforma temporária.
        await repository.ExcluirAsync(id);

        // Confirma a exclusão.
        var excluida = await repository.BuscarPorIdAsync(id);

        Assert.Null(excluida);
    }
    finally
    {
        // Garante a limpeza caso alguma verificação falhe.
        if (id > 0)
            await repository.ExcluirAsync(id);
    }
}
}
