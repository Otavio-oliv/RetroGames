using RetroGames.Model.Model;
using RetroGames.WebApi.ApiModels.Genero;

namespace RetroGames.WebApi.Mappers;

public static class GeneroMapper
{
    public static ListarGeneroResponseApiModel ParaResponse(
        this Genero genero)
    {
        return new ListarGeneroResponseApiModel
        {
            Id = genero.Id,
            Nome = genero.Nome
        };
    }
}
