
namespace RetroGames.WebApi.ApiModels.Pedido;

public class CriarPedidoResponseApiModel
{
    public int Id { get; set; }

    public DateTime DataPedido { get; set; }

    public decimal Total { get; set; }

    public int IdSituacaoPedido { get; set; }
}
