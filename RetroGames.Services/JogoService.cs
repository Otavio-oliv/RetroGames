
using System.Transactions;
using RetroGames.Model.Model;
using RetroGames.Repository.Interfaces;

namespace RetroGames.Services;

public class JogoService : IJogoService
{
    private readonly IRepositorySet _repositorios;
    private readonly IUsuarioAtualService _usuarioAtual;

    public JogoService(
        IRepositorySet repositorios,
        IUsuarioAtualService usuarioAtual)
    {
        _repositorios = repositorios;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<(List<JogoDetalheServiceModel> Jogos, int Total)> ListarAsync(
        string? busca = null,
        int? idPlataforma = null,
        int? idGenero = null,
        int pagina = 1,
        int tamanhoPagina = 10,
        bool incluirInativos = false)
    {
        // Somente administradores podem visualizar jogos inativos.
        var podeIncluirInativos =
            incluirInativos && _usuarioAtual.EhAdministrador;

        IReadOnlyCollection<int>? idsJogos = null;

        // Se houver filtro por gênero, identifica os jogos desse gênero.
        if (idGenero.HasValue)
        {
            var vinculos = await _repositorios.JogoGenero.ListarTodosAsync();

            idsJogos = vinculos
                .Where(v => v.IdGenero == idGenero.Value)
                .Select(v => v.IdJogo)
                .Distinct()
                .ToList();
        }

        // Busca os jogos aplicando os filtros e a paginação.
        var (jogos, total) = await _repositorios.Jogo.ListarAsync(
            busca,
            idPlataforma,
            idsJogos,
            pagina,
            tamanhoPagina,
            podeIncluirInativos);

        var jogosDetalhados = await MontarDetalhesAsync(jogos);

        return (jogosDetalhados, total);
    }

    public async Task<JogoDetalheServiceModel> BuscarPorIdAsync(int id)
    {
        // Busca o jogo pelo identificador.
        var jogo = await _repositorios.Jogo.BuscarPorIdAsync(id);

        // Jogos inexistentes ou inativos para usuários comuns
        // devem ser tratados como não encontrados.
        if (jogo == null ||
            (!jogo.Ativo && !_usuarioAtual.EhAdministrador))
        {
            throw new BusinessRuleException(
                BusinessErrors.JOGO_NAO_ENCONTRADO);
        }

        // Monta os detalhes completos do jogo.
        var detalhes = await MontarDetalhesAsync(new List<Jogo> { jogo });

        return detalhes[0];
    }

    public async Task<JogoDetalheServiceModel> InserirAsync(
        Jogo jogo,
        IReadOnlyCollection<int> idsGeneros)
    {
        // Remove espaços desnecessários do nome.
        jogo.Nome = jogo.Nome.Trim();

        // Verifica se já existe um jogo com o mesmo nome.
        if (await _repositorios.Jogo.ExisteComNomeAsync(jogo.Nome))
            throw new BusinessRuleException(
                BusinessErrors.JOGO_NOME_DUPLICADO);

        // Todo jogo novo começa ativo.
        jogo.Ativo = true;

        // Verifica se a plataforma existe.
        var plataforma = await _repositorios.Plataforma.BuscarPorIdAsync(
            jogo.IdPlataforma);

        if (plataforma == null)
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);

        // Verifica se a condição do jogo existe.
        var condicoes = await _repositorios.CondicaoJogo.ListarTodosAsync();

        if (!condicoes.Any(c => c.Id == jogo.IdCondicaoJogo))
            throw new BusinessRuleException(
                BusinessErrors.CONDICAO_NAO_ENCONTRADA);

        // Remove possíveis gêneros repetidos.
        var idsGenerosUnicos = idsGeneros.Distinct().ToList();

        // Verifica se pelo menos um gênero foi informado.
        if (idsGenerosUnicos.Count == 0)
            throw new BusinessRuleException(
                BusinessErrors.GENERO_NAO_ENCONTRADO);

        // Verifica se todos os gêneros informados existem.
        var generos = await _repositorios.Genero.ListarTodosAsync();

        var idsGenerosExistentes = generos
            .Select(g => g.Id)
            .ToHashSet();

        if (!idsGenerosUnicos.All(id => idsGenerosExistentes.Contains(id)))
            throw new BusinessRuleException(
                BusinessErrors.GENERO_NAO_ENCONTRADO);

        // Configura a transação.
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout,
        };

        using (var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            transactionOptions,
            TransactionScopeAsyncFlowOption.Enabled))
        {
            // Insere o jogo e recupera o ID gerado.
            jogo.Id = await _repositorios.Jogo.InserirAsync(jogo);

            // Monta os vínculos entre o jogo e seus gêneros.
            var vinculos = idsGenerosUnicos
                .Select(idGenero => new JogoGenero
                {
                    IdJogo = jogo.Id,
                    IdGenero = idGenero
                })
                .ToList();

            // Insere os vínculos.
            await _repositorios.JogoGenero.InserirAsync(vinculos);

            // Confirma a transação somente após todas as gravações.
            transactionScope.Complete();
        }

        // Devolve o jogo com todos os seus detalhes.
        return await BuscarPorIdAsync(jogo.Id);
    }

    public async Task AlterarAsync(
        Jogo jogo,
        IReadOnlyCollection<int> idsGeneros)
    {
        // Verifica se o jogo existe.
        var jogoExistente = await _repositorios.Jogo.BuscarPorIdAsync(jogo.Id);

        if (jogoExistente == null)
            throw new BusinessRuleException(
                BusinessErrors.JOGO_NAO_ENCONTRADO);

        // Remove espaços desnecessários do nome.
        jogo.Nome = jogo.Nome.Trim();

        // Verifica se outro jogo já utiliza este nome.
        if (await _repositorios.Jogo.ExisteComNomeAsync(
            jogo.Nome,
            jogo.Id))
        {
            throw new BusinessRuleException(
                BusinessErrors.JOGO_NOME_DUPLICADO);
        }

        // Verifica se a plataforma existe.
        var plataforma = await _repositorios.Plataforma.BuscarPorIdAsync(
            jogo.IdPlataforma);

        if (plataforma == null)
            throw new BusinessRuleException(
                BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);

        // Verifica se a condição existe.
        var condicoes = await _repositorios.CondicaoJogo.ListarTodosAsync();

        if (!condicoes.Any(c => c.Id == jogo.IdCondicaoJogo))
            throw new BusinessRuleException(
                BusinessErrors.CONDICAO_NAO_ENCONTRADA);

        // Remove possíveis gêneros repetidos.
        var idsGenerosUnicos = idsGeneros.Distinct().ToList();

        if (idsGenerosUnicos.Count == 0)
            throw new BusinessRuleException(
                BusinessErrors.GENERO_NAO_ENCONTRADO);

        // Verifica se todos os gêneros existem.
        var generos = await _repositorios.Genero.ListarTodosAsync();

        var idsGenerosExistentes = generos
            .Select(g => g.Id)
            .ToHashSet();

        if (!idsGenerosUnicos.All(id => idsGenerosExistentes.Contains(id)))
            throw new BusinessRuleException(
                BusinessErrors.GENERO_NAO_ENCONTRADO);

        // Configura a transação.
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout,
        };

        using (var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            transactionOptions,
            TransactionScopeAsyncFlowOption.Enabled))
        {
            // Atualiza os dados do jogo.
            await _repositorios.Jogo.AlterarAsync(jogo);

            // Remove os vínculos antigos.
            await _repositorios.JogoGenero.RemoverPorJogoAsync(jogo.Id);

            // Cria os novos vínculos.
            var vinculos = idsGenerosUnicos
                .Select(idGenero => new JogoGenero
                {
                    IdJogo = jogo.Id,
                    IdGenero = idGenero
                })
                .ToList();

            await _repositorios.JogoGenero.InserirAsync(vinculos);

            // Confirma a transação após todas as gravações.
            transactionScope.Complete();
        }
    }

    public async Task DesativarAsync(int id)
    {
        // Verifica se o jogo existe.
        var jogo = await _repositorios.Jogo.BuscarPorIdAsync(id);

        if (jogo == null)
            throw new BusinessRuleException(
                BusinessErrors.JOGO_NAO_ENCONTRADO);

        // Realiza a exclusão lógica.
        await _repositorios.Jogo.DesativarAsync(id);
    }

    private async Task<List<JogoDetalheServiceModel>> MontarDetalhesAsync(
        IReadOnlyCollection<Jogo> jogos)
    {
        if (jogos.Count == 0)
            return new List<JogoDetalheServiceModel>();

        var idsJogos = jogos
            .Select(j => j.Id)
            .ToList();

        var vinculos = await _repositorios.JogoGenero.ListarPorJogosAsync(idsJogos);
        var plataformas = await _repositorios.Plataforma.ListarAsync();
        var condicoes = await _repositorios.CondicaoJogo.ListarTodosAsync();
        var generos = await _repositorios.Genero.ListarTodosAsync();

        var plataformasPorId = plataformas.ToDictionary(p => p.Id);
        var condicoesPorId = condicoes.ToDictionary(c => c.Id);
        var generosPorId = generos.ToDictionary(g => g.Id);

        var generosPorJogo = vinculos
            .GroupBy(v => v.IdJogo)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo
                    .Select(v => generosPorId[v.IdGenero])
                    .ToList());

        return jogos.Select(jogo => new JogoDetalheServiceModel
        {
            Jogo = jogo,
            NomePlataforma = plataformasPorId[jogo.IdPlataforma].Nome,
            DescricaoCondicao = condicoesPorId[jogo.IdCondicaoJogo].Descricao,
            Generos = generosPorJogo.TryGetValue(jogo.Id, out var listaGeneros)
                ? listaGeneros
                : new List<Genero>()
        }).ToList();
    }
}
