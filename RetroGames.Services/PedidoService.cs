using System.Transactions;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Services;

public class PedidoService : IPedidoService
{
    private readonly IRepositorySet _repositorios;
    private readonly IUsuarioAtualService _usuarioAtual;

    public PedidoService(
        IRepositorySet repositorios,
        IUsuarioAtualService usuarioAtual)
    {
        _repositorios = repositorios;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<Pedido> CriarAsync(
        IReadOnlyCollection<ItemPedidoSolicitacaoServiceModel> itens)
    {
        // Verifica os dados recebidos.
        if (itens == null || itens.Count == 0)
            throw new ArgumentException(
                "O pedido deve possuir pelo menos um item.");

        if (itens.Any(item =>
            item.IdJogo <= 0 ||
            item.Quantidade < 1 ||
            item.Quantidade > 10))
        {
            throw new ArgumentException(
                "Cada item deve possuir um jogo válido e quantidade entre 1 e 10.");
        }

        // Impede que o mesmo jogo apareça mais de uma vez.
        if (itens.Select(item => item.IdJogo).Distinct().Count() != itens.Count)
            throw new ArgumentException(
                "Não é permitido repetir o mesmo jogo no pedido.");

        // Consulta os jogos diretamente no banco.
        // O preço utilizado será sempre o preço cadastrado.
        var jogos = new List<(Jogo Jogo, int Quantidade)>();

        foreach (var item in itens)
        {
            var jogo = await _repositorios.Jogo.BuscarPorIdAsync(
                item.IdJogo);

            if (jogo == null || !jogo.Ativo)
            {
                throw new BusinessRuleException(
                    BusinessErrors.JOGO_INDISPONIVEL);
            }

            if (jogo.Estoque < item.Quantidade)
            {
                throw new BusinessRuleException(
                    BusinessErrors.ESTOQUE_INSUFICIENTE);
            }

            jogos.Add((jogo, item.Quantidade));
        }

        // Calcula o total usando os preços consultados no banco.
        var total = jogos.Sum(item =>
            item.Jogo.Preco * item.Quantidade);

        // Monta o pedido.
        var pedido = new Pedido
        {
            DataPedido = DateTime.UtcNow,
            Total = total,
            IdUsuario = _usuarioAtual.IdUsuario,
            IdSituacaoPedido = SituacaoPedidoIds.Pendente
        };

        // Configura a transação.
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout
        };

        using (var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            transactionOptions,
            TransactionScopeAsyncFlowOption.Enabled))
        {
            // Baixa o estoque de cada jogo.
            // A operação SQL verifica novamente se há estoque suficiente.
            foreach (var item in jogos)
            {
                var estoqueBaixado =
                    await _repositorios.Jogo.BaixarEstoqueAsync(
                        item.Jogo.Id,
                        item.Quantidade);

                if (!estoqueBaixado)
                {
                    // O jogo pode ter sido desativado ou comprado
                    // por outra pessoa depois da consulta inicial.
                    var jogoAtualizado =
                        await _repositorios.Jogo.BuscarPorIdAsync(
                            item.Jogo.Id);

                    if (jogoAtualizado == null || !jogoAtualizado.Ativo)
                    {
                        throw new BusinessRuleException(
                            BusinessErrors.JOGO_INDISPONIVEL);
                    }

                    throw new BusinessRuleException(
                        BusinessErrors.ESTOQUE_INSUFICIENTE);
                }
            }

            // Insere o pedido.
            pedido.Id = await _repositorios.Pedido.InserirAsync(pedido);

            // Insere os itens com os preços obtidos do banco.
            foreach (var item in jogos)
            {
                await _repositorios.ItemPedido.InserirAsync(
                    new ItemPedido
                    {
                        Quantidade = item.Quantidade,
                        ValorUnitario = item.Jogo.Preco,
                        IdPedido = pedido.Id,
                        IdJogo = item.Jogo.Id
                    });
            }

            // Confirma somente depois de todas as operações.
            transactionScope.Complete();
        }

        return pedido;
    }

    public async Task<List<PedidoDetalheServiceModel>> ListarMeusAsync()
    {
        // O ID vem da sessão autenticada, nunca de dados enviados pelo cliente.
        var idUsuario = _usuarioAtual.IdUsuario;

        // Busca somente os pedidos pertencentes ao usuário atual.
        // O repositório já retorna os pedidos em ordem decrescente de data.
        var pedidos = await _repositorios.Pedido.ListarPorUsuarioAsync(
            idUsuario);

        var resultado = new List<PedidoDetalheServiceModel>();

        foreach (var pedido in pedidos)
        {
            // Busca a descrição da situação do pedido.
            var situacao =
                await _repositorios.SituacaoPedido.BuscarPorIdAsync(
                    pedido.IdSituacaoPedido);

            if (situacao == null)
            {
                throw new InvalidOperationException(
                    $"A situação {pedido.IdSituacaoPedido} do pedido {pedido.Id} não foi encontrada.");
            }

            // Busca os itens pertencentes ao pedido.
            var itens = await _repositorios.ItemPedido.ListarPorPedidoAsync(
                pedido.Id);

            var itensDetalhados = new List<ItemPedidoDetalheServiceModel>();

            foreach (var item in itens)
            {
                // Busca o jogo para obter seu nome.
                var jogo = await _repositorios.Jogo.BuscarPorIdAsync(
                    item.IdJogo);

                if (jogo == null)
                {
                    throw new InvalidOperationException(
                        $"O jogo {item.IdJogo} do pedido {pedido.Id} não foi encontrado.");
                }

                itensDetalhados.Add(new ItemPedidoDetalheServiceModel
                {
                    Item = item,
                    NomeJogo = jogo.Nome
                });
            }

            // Monta os detalhes do pedido.
            resultado.Add(new PedidoDetalheServiceModel
            {
                Pedido = pedido,
                DescricaoSituacao = situacao.Descricao,
                Itens = itensDetalhados
            });
        }

        return resultado;
    }
}
