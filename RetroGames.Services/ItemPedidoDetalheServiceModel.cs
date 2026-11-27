using RetroGames.Model.Model;

namespace RetroGames.Services;

public class ItemPedidoDetalheServiceModel
{
    public ItemPedido Item { get; set; } = new();

    public string NomeJogo { get; set; } = string.Empty;
}
