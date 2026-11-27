using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels.Pedido;
using RetroGames.WebApi.ApiModels.Responses;

namespace RetroGames.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidoController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidoController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<CriarPedidoResponseApiModel>>> Criar(
        [FromBody] CriarPedidoRequestApiModel request)
    {
        var itens = request.Itens
            .Select(item => new ItemPedidoSolicitacaoServiceModel
            {
                IdJogo = item.IdJogo,
                Quantidade = item.Quantidade
            })
            .ToList();

        var pedido = await _pedidoService.CriarAsync(itens);

        var response = new RetroGamesResponseApiModel<CriarPedidoResponseApiModel>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = new CriarPedidoResponseApiModel
            {
                Id = pedido.Id,
                DataPedido = pedido.DataPedido,
                Total = pedido.Total,
                IdSituacaoPedido = pedido.IdSituacaoPedido
            }
        };

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpGet("meus")]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarMeusPedidosResponseApiModel>>>> ListarMeus()
    {
        var pedidos = await _pedidoService.ListarMeusAsync();

        var response = pedidos
            .Select(detalhe => new ListarMeusPedidosResponseApiModel
            {
                Id = detalhe.Pedido.Id,
                DataPedido = detalhe.Pedido.DataPedido,
                Total = detalhe.Pedido.Total,
                IdSituacaoPedido = detalhe.Pedido.IdSituacaoPedido,
                Situacao = detalhe.DescricaoSituacao,
                Itens = detalhe.Itens
                    .Select(item => new ListarMeusPedidosItemResponseApiModel
                    {
                        IdJogo = item.Item.IdJogo,
                        NomeJogo = item.NomeJogo,
                        Quantidade = item.Item.Quantidade,
                        ValorUnitario = item.Item.ValorUnitario
                    })
                    .ToList()
            })
            .ToList();

        return Ok(new RetroGamesResponseApiModel<List<ListarMeusPedidosResponseApiModel>>
        {
            ResponseCode = ResponseCodes.SUCESSO,
            Data = response
        });
    }
}
