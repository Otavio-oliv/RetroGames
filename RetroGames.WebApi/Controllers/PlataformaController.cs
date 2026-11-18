
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels.Plataforma;
using RetroGames.WebApi.ApiModels.Responses;
using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlataformaController : ControllerBase
{
    private readonly IPlataformaService _plataformaService;

    public PlataformaController(IPlataformaService plataformaService)
    {
        _plataformaService = plataformaService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarPlataformaResponseApiModel>>>> Listar()
    {
        var plataformas = await _plataformaService.ListarAsync();

        var response = new RetroGamesResponseApiModel<List<ListarPlataformaResponseApiModel>>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = plataformas.Select(p => p.ParaResponse()).ToList()
        };

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>>> BuscarPorId(
        int id)
    {
        var plataforma = await _plataformaService.BuscarPorIdAsync(id);

        return Ok(new RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = plataforma.ParaResponse()
        });
    }

    [Authorize(Policy = Politicas.Administrador)]
    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>>> Inserir(
        [FromBody] InserirPlataformaRequestApiModel request)
    {
        var plataforma = request.ParaModel();

        var criada = await _plataformaService.InserirAsync(plataforma);

        var response = new RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = criada.ParaResponse()
        };

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = criada.Id },
            response);
    }

    [Authorize(Policy = Politicas.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<object>>> Alterar(
        int id,
        [FromBody] AlterarPlataformaRequestApiModel request)
    {
        var plataforma = request.ParaModel(id);

        await _plataformaService.AlterarAsync(plataforma);

        return Ok(new RetroGamesResponseApiModel<object>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = null
        });
    }

    [Authorize(Policy = Politicas.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<object>>> Excluir(
        int id)
    {
        await _plataformaService.ExcluirAsync(id);

        return Ok(new RetroGamesResponseApiModel<object>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = null
        });
    }
}
