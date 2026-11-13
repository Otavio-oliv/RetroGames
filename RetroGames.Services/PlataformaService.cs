using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Repository.Interfaces;
using RetroGames.Repository.Exceptions;

namespace RetroGames.Services;

public class PlataformaService : IPlataformaService
{
    private readonly IRepositorySet _repositorios;

    public PlataformaService(IRepositorySet repositorios)
    {
        _repositorios = repositorios;
    }

    public async Task<List<Plataforma>> ListarAsync()
    {
        return await _repositorios.Plataforma.ListarAsync();
    }

    public async Task<Plataforma> BuscarPorIdAsync(int id)
    {
        var plataforma = await _repositorios.Plataforma.BuscarPorIdAsync(id);

        if (plataforma == null)
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);

        return plataforma;
    }

    public async Task<Plataforma> InserirAsync(Plataforma plataforma)
    {
        plataforma.Nome = plataforma.Nome.Trim();

        if (await _repositorios.Plataforma.ExisteComNomeAsync(plataforma.Nome))
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NOME_DUPLICADO);

        try
        {
            plataforma.Id = await _repositorios.Plataforma.InserirAsync(plataforma);
        }
        catch (RegistroDuplicadoException)
        {
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NOME_DUPLICADO);
        }

        return plataforma;
    }

    public async Task AlterarAsync(Plataforma plataforma)
    {
        var existente = await _repositorios.Plataforma.BuscarPorIdAsync(plataforma.Id);

        if (existente == null)
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);

        plataforma.Nome = plataforma.Nome.Trim();

        if (await _repositorios.Plataforma.ExisteComNomeAsync(
                plataforma.Nome, plataforma.Id))
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NOME_DUPLICADO);

        try
        {
            await _repositorios.Plataforma.AlterarAsync(plataforma);
        }
        catch (RegistroDuplicadoException)
        {
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NOME_DUPLICADO);
        }
    }

    public async Task ExcluirAsync(int id)
    {
        var plataforma = await _repositorios.Plataforma.BuscarPorIdAsync(id);

        if (plataforma == null)
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);

        if (await _repositorios.Plataforma.PossuiJogosAsync(id))
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_POSSUI_JOGOS);

        await _repositorios.Plataforma.ExcluirAsync(id);
    }
}
