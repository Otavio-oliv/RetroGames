using RetroGames.Model.Model;
using RetroGames.Repository.Config;
using RetroGames.Repository.Repositories;

namespace RetroGames.WebApi.UnitTests;

public class JogoGeneroRepositoryTests
{
    private static JogoGeneroRepository CriarRepository()
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

        return new JogoGeneroRepository(config);
    }

    [Fact]
    public async Task DeveListarTodosOsVinculos()
    {
        var repository = CriarRepository();

        var vinculos = await repository.ListarTodosAsync();

        Assert.NotNull(vinculos);
        Assert.NotEmpty(vinculos);

        Assert.All(vinculos, vinculo =>
        {
            Assert.True(vinculo.IdJogo > 0);
            Assert.True(vinculo.IdGenero > 0);
        });
    }

    [Fact]
    public async Task DeveListarVinculosDeVariosJogos()
    {
        var repository = CriarRepository();

        var todosVinculos = await repository.ListarTodosAsync();

        Assert.NotEmpty(todosVinculos);

        var idsJogos = todosVinculos
            .Select(vinculo => vinculo.IdJogo)
            .Distinct()
            .Take(3)
            .ToList();

        var vinculosFiltrados =
            await repository.ListarPorJogosAsync(idsJogos);

        var vinculosEsperados = todosVinculos
            .Where(vinculo => idsJogos.Contains(vinculo.IdJogo))
            .OrderBy(vinculo => vinculo.IdJogo)
            .ThenBy(vinculo => vinculo.IdGenero)
            .ToList();

        var vinculosOrdenados = vinculosFiltrados
            .OrderBy(vinculo => vinculo.IdJogo)
            .ThenBy(vinculo => vinculo.IdGenero)
            .ToList();

        Assert.Equal(vinculosEsperados.Count, vinculosOrdenados.Count);

        Assert.Equal(
            vinculosEsperados.Select(v => (v.IdJogo, v.IdGenero)),
            vinculosOrdenados.Select(v => (v.IdJogo, v.IdGenero)));
    }

    [Fact]
    public async Task DeveRetornarListaVaziaQuandoNaoReceberIds()
    {
        var repository = CriarRepository();

        var vinculos = await repository.ListarPorJogosAsync(
            new List<int>());

        Assert.NotNull(vinculos);
        Assert.Empty(vinculos);
    }
    [Fact]
    public async Task DeveInserirERemoverVinculosRestaurandoDadosOriginais()
    {
        var repository = CriarRepository();

        const int idJogo = 1;

        var vinculosOriginais =
            await repository.ListarPorJogosAsync(
                new List<int> { idJogo });

        Assert.NotEmpty(vinculosOriginais);

        var novoVinculo = new JogoGenero
        {
            IdJogo = idJogo,
            IdGenero = 8
        };

        Assert.DoesNotContain(
            vinculosOriginais,
            v => v.IdGenero == novoVinculo.IdGenero);

        try
        {
            await repository.InserirAsync(
                new List<JogoGenero> { novoVinculo });

            var vinculosAposInsercao =
                await repository.ListarPorJogosAsync(
                    new List<int> { idJogo });

            Assert.Contains(
                vinculosAposInsercao,
                v => v.IdGenero == 8);

            await repository.RemoverPorJogoAsync(idJogo);

            var vinculosAposRemocao =
                await repository.ListarPorJogosAsync(
                    new List<int> { idJogo });

            Assert.Empty(vinculosAposRemocao);
        }
        finally
        {
            await repository.RemoverPorJogoAsync(idJogo);

            await repository.InserirAsync(vinculosOriginais);
        }

        var vinculosRestaurados =
            await repository.ListarPorJogosAsync(
                new List<int> { idJogo });

        Assert.Equal(
            vinculosOriginais.Select(v => v.IdGenero).OrderBy(id => id),
            vinculosRestaurados.Select(v => v.IdGenero).OrderBy(id => id));
    }
}
