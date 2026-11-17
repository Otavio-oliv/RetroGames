using RetroGames.Model.Model;
using RetroGames.WebApi.ApiModels.CondicaoJogo;

namespace RetroGames.WebApi.Mappers;

public static class CondicaoJogoMapper
{
    public static ListarCondicaoJogoResponseApiModel ParaResponse(
        this CondicaoJogo condicaoJogo)
    {
        return new ListarCondicaoJogoResponseApiModel
        {
            Id = condicaoJogo.Id,
            Descricao = condicaoJogo.Descricao
        };
    }
}
