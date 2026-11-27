
namespace RetroGames.WebApi.ApiModels.Pedido;

public class CriarPedidoRequestApiModel
{
    public List<CriarPedidoItemRequestApiModel> Itens { get; set; } = [];
}

public class CriarPedidoItemRequestApiModel
{
    public int IdJogo { get; set; }

    public int Quantidade { get; set; }
}
