using RetroGames.Model.Model;

namespace RetroGames.Services;

public class PedidoDetalheServiceModel
{
    public Pedido Pedido { get; set; } = new();

    public string DescricaoSituacao { get; set; } = string.Empty;

    public List<ItemPedidoDetalheServiceModel> Itens { get; set; } = [];
}
