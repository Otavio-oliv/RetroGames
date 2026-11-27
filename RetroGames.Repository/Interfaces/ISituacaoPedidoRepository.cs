
using RetroGames.Model.Model;

namespace RetroGames.Repository.Interfaces;

public interface ISituacaoPedidoRepository
{
    Task<SituacaoPedido?> BuscarPorIdAsync(int id);

    Task<List<SituacaoPedido>> ListarTodosAsync();
}
