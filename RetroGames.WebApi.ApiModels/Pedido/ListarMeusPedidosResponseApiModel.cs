namespace RetroGames.WebApi.ApiModels.Pedido;

public class ListarMeusPedidosResponseApiModel
{
    public int Id { get; set; }

    public DateTime DataPedido { get; set; }

    public decimal Total { get; set; }

    public int IdSituacaoPedido { get; set; }

    public string Situacao { get; set; } = string.Empty;

    public List<ListarMeusPedidosItemResponseApiModel> Itens { get; set; } = [];
}
