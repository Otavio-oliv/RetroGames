using RetroGames.Model.Model;

namespace RetroGames.Services;

public interface IPedidoService
{
    Task<Pedido> CriarAsync(
        IReadOnlyCollection<ItemPedidoSolicitacaoServiceModel> itens);

    Task<List<PedidoDetalheServiceModel>> ListarMeusAsync();
}
