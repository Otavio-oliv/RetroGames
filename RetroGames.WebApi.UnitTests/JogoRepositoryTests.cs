
using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Repositories;

namespace RetroGames.WebApi.UnitTests;

public class JogoRepositoryTests
{
    private static JogoRepository CriarRepository()
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

        return new JogoRepository(config);
    }

    [Fact]
    public async Task DeveListarJogosComPaginacaoEContagem()
    {
        var repository = CriarRepository();

        var (jogos, total) = await repository.ListarAsync(
            pagina: 1,
            tamanhoPagina: 5);

        Assert.Equal(19, total);
        Assert.Equal(5, jogos.Count);
        Assert.Equal("Crash Bandicoot", jogos[0].Nome);

        var (segundaPagina, segundoTotal) =
            await repository.ListarAsync(
                pagina: 2,
                tamanhoPagina: 5);

        Assert.Equal(19, segundoTotal);
        Assert.Equal(5, segundaPagina.Count);
        Assert.Equal("Metroid", segundaPagina[0].Nome);
    }

    [Fact]
    public async Task DeveFiltrarJogosPorNome()
    {
        var repository = CriarRepository();

        var (jogos, total) = await repository.ListarAsync(
            busca: "Mega");

        Assert.Equal(1, total);
        Assert.Single(jogos);
        Assert.Equal("Mega Man 2", jogos[0].Nome);
    }

    [Fact]
    public async Task DeveFiltrarJogosPorPlataforma()
    {
        var repository = CriarRepository();

        var (jogos, total) = await repository.ListarAsync(
            idPlataforma: 1);

        Assert.Equal(4, total);
        Assert.Equal(4, jogos.Count);
        Assert.All(
            jogos,
            jogo => Assert.Equal(1, jogo.IdPlataforma));
    }

    [Fact]
    public async Task DeveFiltrarJogosPorListaDeIds()
    {
        var repository = CriarRepository();

        var ids = new List<int> { 16, 7, 11 };

        var (jogos, total) = await repository.ListarAsync(
            idsJogos: ids);

        Assert.Equal(3, total);
        Assert.Equal(3, jogos.Count);

        Assert.Equal(
            ids.OrderBy(id => id),
            jogos.Select(jogo => jogo.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task DeveRespeitarFiltroDeJogosInativos()
    {
        var repository = CriarRepository();

        var (jogosAtivos, totalAtivos) =
            await repository.ListarAsync(
                busca: "Pokémon Red");

        Assert.Equal(0, totalAtivos);
        Assert.Empty(jogosAtivos);

        var (todosJogos, totalTodos) =
            await repository.ListarAsync(
                busca: "Pokémon Red",
                incluirInativos: true);

        Assert.Equal(1, totalTodos);
        Assert.Single(todosJogos);
        Assert.Equal("Pokémon Red", todosJogos[0].Nome);
        Assert.False(todosJogos[0].Ativo);
    }
}

