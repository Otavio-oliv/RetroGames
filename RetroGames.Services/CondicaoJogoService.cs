using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Services;

public class CondicaoJogoService : ICondicaoJogoService
{
    private readonly IRepositorySet _repositorios;

    public CondicaoJogoService(IRepositorySet repositorios)
    {
        _repositorios = repositorios;
    }

    public async Task<List<CondicaoJogo>> ListarTodosAsync()
    {
        return await _repositorios.CondicaoJogo.ListarTodosAsync();
    }
}

