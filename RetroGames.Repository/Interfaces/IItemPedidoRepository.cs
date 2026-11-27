using RetroGames.Model.Model;

namespace RetroGames.Repository.Interfaces;

public interface IItemPedidoRepository
{
    Task InserirAsync(ItemPedido item);

    Task<List<ItemPedido>> ListarPorPedidoAsync(int idPedido);
}
