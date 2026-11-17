
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels.Jogo;
using RetroGames.WebApi.ApiModels.Responses;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JogoController : ControllerBase
{
    private readonly IJogoService _jogoService;

    public JogoController(IJogoService jogoService)
    {
        _jogoService = jogoService;
    }

    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<PaginaResponseApiModel<ListarJogoResponseApiModel>>>> Listar(
        [FromQuery] ListarJogoRequestApiModel request)
    {
        var (jogos, total) = await _jogoService.ListarAsync(
            request.Busca,
            request.IdPlataforma,
            request.IdGenero,
            request.Pagina,
            request.TamanhoPagina);

        var response = new PaginaResponseApiModel<ListarJogoResponseApiModel>
        {
            Itens = jogos
                .Select(jogo => jogo.ParaResponse())
                .ToList(),

            Pagina = request.Pagina,
            TamanhoPagina = request.TamanhoPagina,
            TotalItens = total,

            TotalPaginas = (int)Math.Ceiling(
                (double)total / request.TamanhoPagina)
        };

        return Ok(new RetroGamesResponseApiModel<PaginaResponseApiModel<ListarJogoResponseApiModel>>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = response
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>>> ListarPorId(
        int id)
    {
        var jogo = await _jogoService.BuscarPorIdAsync(id);

        return Ok(new RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = jogo.ParaDetalheResponse()
        });
    }

    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>>> Inserir(
        [FromBody] InserirJogoRequestApiModel request)
    {
        var jogo = await _jogoService.InserirAsync(
            request.ParaModel(),
            request.IdsGeneros);

        var response = new RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = jogo.ParaDetalheResponse()
        };

        return CreatedAtAction(
            nameof(ListarPorId),
            new { id = jogo.Jogo.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<object>>> Alterar(
        int id,
        [FromBody] AlterarJogoRequestApiModel request)
    {
        await _jogoService.AlterarAsync(
            request.ParaModel(id),
            request.IdsGeneros);

        return Ok(new RetroGamesResponseApiModel<object>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = null
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<object>>> Excluir(
        int id)
    {
        await _jogoService.DesativarAsync(id);

        return Ok(new RetroGamesResponseApiModel<object>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = null
        });
    }
}
