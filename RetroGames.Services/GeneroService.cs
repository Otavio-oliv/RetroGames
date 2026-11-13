using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Services;

public class GeneroService : IGeneroService
{
    private readonly IRepositorySet _repositorios;

    public GeneroService(IRepositorySet repositorios)
    {
        _repositorios = repositorios;
    }

    public async Task<List<Genero>> ListarTodosAsync()
    {
        return await _repositorios.Genero.ListarTodosAsync();
    }
}
