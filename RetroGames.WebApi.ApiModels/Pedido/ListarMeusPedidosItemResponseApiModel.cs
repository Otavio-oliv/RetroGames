namespace RetroGames.WebApi.ApiModels.Pedido;

public class ListarMeusPedidosItemResponseApiModel
{
    public int IdJogo { get; set; }

    public string NomeJogo { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }
}
