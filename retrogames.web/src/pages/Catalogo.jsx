import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

import CardJogo from '../components/CardJogo'
import Paginacao from '../components/Paginacao'
import {
  Carregando,
  MensagemErro,
  EstadoVazio,
} from '../components/MensagensEstado'

import {
  jogoApi,
  plataformaApi,
  generoApi,
} from '../services/servicos'

import { obterMensagemErro } from '../services/erros'

const TAMANHO_PAGINA = 12

function Catalogo() {
  const [searchParams, setSearchParams] = useSearchParams()

  const buscaUrl = searchParams.get('busca') || ''
  const idPlataforma = searchParams.get('idPlataforma') || ''
  const idGenero = searchParams.get('idGenero') || ''

  const pagina = Math.max(
    1,
    Number(searchParams.get('pagina')) || 1
  )

  const [busca, setBusca] = useState(buscaUrl)

  const [plataformas, setPlataformas] = useState([])
  const [generos, setGeneros] = useState([])

  const [jogos, setJogos] = useState([])
  const [totalItens, setTotalItens] = useState(0)
  const [totalPaginas, setTotalPaginas] = useState(0)

  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [erroFiltros, setErroFiltros] = useState('')

  const [tentativa, setTentativa] = useState(0)

  // Mantém o campo de busca sincronizado com a URL.
  useEffect(() => {
    setBusca(buscaUrl)
  }, [buscaUrl])

  // Carrega as opções dos filtros uma única vez.
  useEffect(() => {
    let ativo = true

    async function carregarFiltros() {
      try {
        const [listaPlataformas, listaGeneros] = await Promise.all([
          plataformaApi.listar(),
          generoApi.listar(),
        ])

        if (ativo) {
          setPlataformas(listaPlataformas)
          setGeneros(listaGeneros)
          setErroFiltros('')
        }
      } catch (erro) {
        if (ativo) {
          setErroFiltros(obterMensagemErro(erro))
        }
      }
    }

    carregarFiltros()

    return () => {
      ativo = false
    }
  }, [])

  // Busca os jogos sempre que os filtros ou a página mudarem.
  useEffect(() => {
    let ativo = true

    async function carregarJogos() {
      setCarregando(true)
      setErro('')

      const parametros = {
        pagina,
        tamanhoPagina: TAMANHO_PAGINA,
      }

      if (buscaUrl.trim()) {
        parametros.busca = buscaUrl.trim()
      }

      if (idPlataforma) {
        parametros.idPlataforma = Number(idPlataforma)
      }

      if (idGenero) {
        parametros.idGenero = Number(idGenero)
      }

      try {
        const resultado = await jogoApi.listar(parametros)

        if (ativo) {
          setJogos(resultado.itens)
          setTotalItens(resultado.totalItens)
          setTotalPaginas(resultado.totalPaginas)
        }
      } catch (erro) {
        if (ativo) {
          setErro(obterMensagemErro(erro))
          setJogos([])
          setTotalItens(0)
          setTotalPaginas(0)
        }
      } finally {
        if (ativo) {
          setCarregando(false)
        }
      }
    }

    carregarJogos()

    return () => {
      ativo = false
    }
  }, [buscaUrl, idPlataforma, idGenero, pagina, tentativa])

  // Atualiza um filtro na URL e retorna para a primeira página.
  function atualizarFiltro(nome, valor) {
    const novosParametros = new URLSearchParams(searchParams)

    if (valor) {
      novosParametros.set(nome, valor)
    } else {
      novosParametros.delete(nome)
    }

    novosParametros.set('pagina', '1')

    setSearchParams(novosParametros)
  }

  // Aplica a busca por texto.
  function handleBuscar(event) {
    event.preventDefault()

    const novosParametros = new URLSearchParams(searchParams)

    if (busca.trim()) {
      novosParametros.set('busca', busca.trim())
    } else {
      novosParametros.delete('busca')
    }

    novosParametros.set('pagina', '1')

    setSearchParams(novosParametros)
  }

  // Altera a página mantendo os filtros atuais.
  function mudarPagina(novaPagina) {
    const novosParametros = new URLSearchParams(searchParams)

    novosParametros.set('pagina', String(novaPagina))

    setSearchParams(novosParametros)
  }

  function tentarNovamente() {
    setTentativa((valorAtual) => valorAtual + 1)
  }

  return (
    <div>
      <div className="mb-4">
        <h1 className="h2 fw-bold mb-2">Catálogo de Jogos</h1>

        <p className="text-secondary">
          Explore nossa coleção de jogos clássicos.
        </p>
      </div>

      {/* Filtros */}
      <form
        onSubmit={handleBuscar}
        className="card card-body shadow-sm mb-4"
      >
        <div className="row g-3">
          <div className="col-12 col-md-6">
            <label htmlFor="busca" className="form-label">
              Buscar jogo
            </label>

            <input
              id="busca"
              type="search"
              className="form-control"
              placeholder="Digite o nome do jogo..."
              value={busca}
              onChange={(event) => setBusca(event.target.value)}
            />
          </div>

          <div className="col-12 col-md-3">
            <label htmlFor="plataforma" className="form-label">
              Plataforma
            </label>

            <select
              id="plataforma"
              className="form-select"
              value={idPlataforma}
              onChange={(event) =>
                atualizarFiltro('idPlataforma', event.target.value)
              }
            >
              <option value="">Todas as plataformas</option>

              {plataformas.map((plataforma) => (
                <option key={plataforma.id} value={plataforma.id}>
                  {plataforma.nome}
                </option>
              ))}
            </select>
          </div>

          <div className="col-12 col-md-3">
            <label htmlFor="genero" className="form-label">
              Gênero
            </label>

            <select
              id="genero"
              className="form-select"
              value={idGenero}
              onChange={(event) =>
                atualizarFiltro('idGenero', event.target.value)
              }
            >
              <option value="">Todos os gêneros</option>

              {generos.map((genero) => (
                <option key={genero.id} value={genero.id}>
                  {genero.nome}
                </option>
              ))}
            </select>
          </div>

          <div className="col-12 d-flex gap-2">
            <button type="submit" className="btn btn-primary">
              Buscar
            </button>

            <button
              type="button"
              className="btn btn-outline-secondary"
              onClick={() => {
                setBusca('')
                setSearchParams({})
              }}
            >
              Limpar filtros
            </button>
          </div>
        </div>
      </form>

      {erroFiltros && (
        <MensagemErro mensagem={`Erro ao carregar os filtros: ${erroFiltros}`} />
      )}

      {/* Resultado da busca */}
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="h5 mb-0">Jogos encontrados</h2>

        {!carregando && !erro && (
          <span className="text-secondary small">
            {totalItens} {totalItens === 1 ? 'jogo encontrado' : 'jogos encontrados'}
          </span>
        )}
      </div>

      {carregando ? (
        <Carregando mensagem="Carregando jogos..." />
      ) : erro ? (
        <MensagemErro
          mensagem={erro}
          aoTentarNovamente={tentarNovamente}
        />
      ) : jogos.length === 0 ? (
        <EstadoVazio mensagem="Nenhum jogo encontrado com os filtros selecionados." />
      ) : (
        <>
          <div className="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-4">
            {jogos.map((jogo) => (
              <div className="col" key={jogo.id}>
                <CardJogo jogo={jogo} />
              </div>
            ))}
          </div>

          <div className="mt-4">
            <Paginacao
              pagina={pagina}
              totalPaginas={totalPaginas}
              aoMudarPagina={mudarPagina}
            />
          </div>
        </>
      )}
    </div>
  )
}

export default Catalogo
