import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'

import {
  jogoApi,
  plataformaApi,
  generoApi,
} from '../../services/servicos'

import { obterMensagemErro } from '../../services/erros'

import Paginacao from '../../components/Paginacao'

import {
  Carregando,
  MensagemErro,
  EstadoVazio,
} from '../../components/MensagensEstado'

function Jogos() {
  const [jogos, setJogos] = useState({
    itens: [],
    pagina: 1,
    totalPaginas: 0,
    totalItens: 0,
  })

  const [plataformas, setPlataformas] = useState([])
  const [generos, setGeneros] = useState([])

  const [busca, setBusca] = useState('')
  const [buscaAplicada, setBuscaAplicada] = useState('')
  const [idPlataforma, setIdPlataforma] = useState('')
  const [idGenero, setIdGenero] = useState('')

  const [pagina, setPagina] = useState(1)
  const [tentativa, setTentativa] = useState(0)

  const [estaCarregando, setEstaCarregando] = useState(true)
  const [mensagemErro, setMensagemErro] = useState('')
  const [mensagemAcao, setMensagemAcao] = useState('')

  useEffect(() => {
    let ativo = true

    async function carregarFiltros() {
      try {
        const [listaPlataformas, listaGeneros] = await Promise.all([
          plataformaApi.listar(),
          generoApi.listar(),
        ])

        if (!ativo) {
          return
        }

        setPlataformas(listaPlataformas)
        setGeneros(listaGeneros)
      } catch {
        if (ativo) {
          setMensagemAcao(
            'Não foi possível carregar as opções de filtro.'
          )
        }
      }
    }

    carregarFiltros()

    return () => {
      ativo = false
    }
  }, [])

  useEffect(() => {
    let ativo = true

    async function carregarJogos() {
      setEstaCarregando(true)
      setMensagemErro('')

      try {
        const parametros = {
          pagina,
          tamanhoPagina: 12,
          incluirInativos: true,
        }

        if (buscaAplicada.trim()) {
          parametros.busca = buscaAplicada.trim()
        }

        if (idPlataforma) {
          parametros.idPlataforma = Number(idPlataforma)
        }

        if (idGenero) {
          parametros.idGenero = Number(idGenero)
        }

        const resultado = await jogoApi.listar(parametros)

        if (!ativo) {
          return
        }

        setJogos(resultado)
      } catch (erro) {
        if (ativo) {
          setMensagemErro(obterMensagemErro(erro))
        }
      } finally {
        if (ativo) {
          setEstaCarregando(false)
        }
      }
    }

    carregarJogos()

    return () => {
      ativo = false
    }
  }, [
    pagina,
    buscaAplicada,
    idPlataforma,
    idGenero,
    tentativa,
  ])

  function pesquisar(evento) {
    evento.preventDefault()
    setPagina(1)
    setBuscaAplicada(busca)
  }

  async function desativarJogo(jogo) {
    const confirmou = window.confirm(
      `Deseja realmente desativar o jogo "${jogo.nome}"?`
    )

    if (!confirmou) {
      return
    }

    setMensagemAcao('')

    try {
      await jogoApi.desativar(jogo.id)

      setMensagemAcao('Jogo desativado com sucesso.')
      setTentativa((valor) => valor + 1)
    } catch (erro) {
      setMensagemAcao(obterMensagemErro(erro))
    }
  }

  const formatarPreco = (preco) => {
    return Number(preco).toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    })
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center flex-wrap gap-3 mb-4">
        <div>
          <h1>Administração de jogos</h1>

          <p className="text-secondary mb-0">
            Gerenciamento dos jogos cadastrados no RetroGames.
          </p>
        </div>

        <Link
          to="/admin/jogos/novo"
          className="btn btn-primary"
        >
          Novo jogo
        </Link>
      </div>

      {mensagemAcao && (
        <div className="alert alert-info" role="alert">
          {mensagemAcao}
        </div>
      )}

      <form onSubmit={pesquisar} className="card card-body mb-4">
        <div className="row g-3">
          <div className="col-md-4">
            <label htmlFor="busca" className="form-label">
              Buscar jogo
            </label>

            <input
              id="busca"
              type="search"
              className="form-control"
              placeholder="Digite o nome do jogo"
              value={busca}
              onChange={(evento) => setBusca(evento.target.value)}
            />
          </div>

          <div className="col-md-3">
            <label htmlFor="filtroPlataforma" className="form-label">
              Plataforma
            </label>

            <select
              id="filtroPlataforma"
              className="form-select"
              value={idPlataforma}
              onChange={(evento) => {
                setIdPlataforma(evento.target.value)
                setPagina(1)
              }}
            >
              <option value="">Todas as plataformas</option>

              {plataformas.map((plataforma) => (
                <option key={plataforma.id} value={plataforma.id}>
                  {plataforma.nome}
                </option>
              ))}
            </select>
          </div>

          <div className="col-md-3">
            <label htmlFor="filtroGenero" className="form-label">
              Gênero
            </label>

            <select
              id="filtroGenero"
              className="form-select"
              value={idGenero}
              onChange={(evento) => {
                setIdGenero(evento.target.value)
                setPagina(1)
              }}
            >
              <option value="">Todos os gêneros</option>

              {generos.map((genero) => (
                <option key={genero.id} value={genero.id}>
                  {genero.nome}
                </option>
              ))}
            </select>
          </div>

          <div className="col-md-2 d-flex align-items-end">
            <button type="submit" className="btn btn-primary w-100">
              Buscar
            </button>
          </div>
        </div>
      </form>

      {estaCarregando ? (
        <Carregando mensagem="Carregando jogos..." />
      ) : mensagemErro ? (
        <MensagemErro
          mensagem={mensagemErro}
          aoTentarNovamente={() => setTentativa((valor) => valor + 1)}
        />
      ) : jogos.itens.length === 0 ? (
        <EstadoVazio mensagem="Nenhum jogo encontrado." />
      ) : (
        <>
          <p className="text-secondary">
            Total de jogos encontrados: {jogos.totalItens}
          </p>

          <div className="table-responsive">
            <table className="table table-striped table-hover align-middle">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Ano</th>
                  <th>Plataforma</th>
                  <th>Preço</th>
                  <th>Estoque</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              </thead>

              <tbody>
                {jogos.itens.map((jogo) => (
                  <tr key={jogo.id}>
                    <td>{jogo.nome}</td>

                    <td>{jogo.anoLancamento}</td>

                    <td>{jogo.nomePlataforma}</td>

                    <td>{formatarPreco(jogo.preco)}</td>

                    <td>{jogo.estoque}</td>

                    <td>
                      {jogo.ativo ? (
                        <span className="badge text-bg-success">
                          Ativo
                        </span>
                      ) : (
                        <span className="badge text-bg-secondary">
                          Inativo
                        </span>
                      )}
                    </td>

                    <td>
                      <div className="d-flex gap-2">
                        <Link
                          to={`/admin/jogos/${jogo.id}/editar`}
                          className="btn btn-sm btn-outline-primary"
                        >
                          Editar
                        </Link>

                        {jogo.ativo && (
                          <button
                            type="button"
                            className="btn btn-sm btn-outline-danger"
                            onClick={() => desativarJogo(jogo)}
                          >
                            Desativar
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <Paginacao
            pagina={jogos.pagina}
            totalPaginas={jogos.totalPaginas}
            aoMudarPagina={setPagina}
          />
        </>
      )}
    </div>
  )
}

export default Jogos
