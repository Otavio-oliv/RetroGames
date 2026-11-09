using RetroGames.Model.Model;

namespace RetroGames.Repository.Interfaces;

public interface IJogoRepository
{
    Task<(List<Jogo> Jogos, int Total)> ListarAsync(
        string? busca = null,
        int? idPlataforma = null,
        IReadOnlyCollection<int>? idsJogos = null,
        int pagina = 1,
        int tamanhoPagina = 10,
        bool incluirInativos = false);

    Task<Jogo?> BuscarPorIdAsync(int id);

    Task<int> InserirAsync(Jogo jogo);

    Task AlterarAsync(Jogo jogo);

    Task DesativarAsync(int id);
}
