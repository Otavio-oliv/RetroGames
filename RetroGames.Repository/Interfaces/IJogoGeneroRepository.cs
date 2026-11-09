
using RetroGames.Model.Model;

namespace RetroGames.Repository.Interfaces;

public interface IJogoGeneroRepository
{
    Task<List<JogoGenero>> ListarTodosAsync();

    Task<List<JogoGenero>> ListarPorJogosAsync(
        IReadOnlyCollection<int> idsJogos);

    Task InserirAsync(
        IReadOnlyCollection<JogoGenero> vinculos);

    Task RemoverPorJogoAsync(int idJogo);
}
