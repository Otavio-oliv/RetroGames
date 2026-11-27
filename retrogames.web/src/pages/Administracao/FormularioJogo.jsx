import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'

import {
  jogoApi,
  plataformaApi,
  generoApi,
  condicaoJogoApi,
} from '../../services/servicos'

import {
  obterMensagemErro,
  obterErrosDosCampos,
} from '../../services/erros'

import {
  Carregando,
  MensagemErro,
} from '../../components/MensagensEstado'

const formularioInicial = {
  nome: '',
  descricao: '',
  anoLancamento: '',
  preco: '',
  estoque: '',
  idPlataforma: '',
  idCondicaoJogo: '',
  idsGeneros: [],
  ativo: true,
}

function FormularioJogo() {
  const { id } = useParams()
  const navigate = useNavigate()

  const estaEditando = Boolean(id)

  const [formulario, setFormulario] = useState(formularioInicial)

  const [plataformas, setPlataformas] = useState([])
  const [generos, setGeneros] = useState([])
  const [condicoes, setCondicoes] = useState([])

  const [estaCarregando, setEstaCarregando] = useState(true)
  const [estaSalvando, setEstaSalvando] = useState(false)

  const [mensagemErro, setMensagemErro] = useState('')
  const [errosCampos, setErrosCampos] = useState({})

  useEffect(() => {
    let ativo = true

    async function carregarDados() {
      setEstaCarregando(true)
      setMensagemErro('')

      try {
        const [
          listaPlataformas,
          listaGeneros,
          listaCondicoes,
          jogo,
        ] = await Promise.all([
          plataformaApi.listar(),
          generoApi.listar(),
          condicaoJogoApi.listar(),
          estaEditando
            ? jogoApi.buscarPorId(id)
            : Promise.resolve(null),
        ])

        if (!ativo) {
          return
        }

        setPlataformas(listaPlataformas)
        setGeneros(listaGeneros)
        setCondicoes(listaCondicoes)

        if (jogo) {
          setFormulario({
            nome: jogo.nome ?? '',
            descricao: jogo.descricao ?? '',
            anoLancamento: String(jogo.anoLancamento ?? ''),
            preco: String(jogo.preco ?? ''),
            estoque: String(jogo.estoque ?? ''),
            idPlataforma: String(jogo.idPlataforma ?? ''),
            idCondicaoJogo: String(jogo.idCondicaoJogo ?? ''),
            idsGeneros: (jogo.generos ?? []).map(
              (genero) => String(genero.id)
            ),
            ativo: jogo.ativo,
          })
        }
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

    carregarDados()

    return () => {
      ativo = false
    }
  }, [id, estaEditando])

  function atualizarCampo(evento) {
    const { name, value } = evento.target

    setFormulario((atual) => ({
      ...atual,
      [name]: value,
    }))

    setErrosCampos({})
    setMensagemErro('')
  }

  function atualizarGenero(idGenero, selecionado) {
    setFormulario((atual) => {
      const idsAtuais = atual.idsGeneros

      const novosIds = selecionado
        ? [...idsAtuais, String(idGenero)]
        : idsAtuais.filter((idAtual) => idAtual !== String(idGenero))

      return {
        ...atual,
        idsGeneros: novosIds,
      }
    })

    setErrosCampos({})
    setMensagemErro('')
  }

  function obterErroCampo(nome) {
    const chaveMinuscula =
      nome.charAt(0).toLowerCase() + nome.slice(1)

    return errosCampos[nome] || errosCampos[chaveMinuscula] || ''
  }

  function obterErroGeneros() {
    const erroDireto =
      obterErroCampo('IdsGeneros')

    if (erroDireto) {
      return erroDireto
    }

    const entrada = Object.entries(errosCampos).find(
      ([campo]) => campo.toLowerCase().startsWith('idsgeneros')
    )

    return entrada?.[1] || ''
  }

  async function salvar(evento) {
    evento.preventDefault()

    setMensagemErro('')
    setErrosCampos({})
    setEstaSalvando(true)

    const dados = {
      nome: formulario.nome.trim(),
      descricao: formulario.descricao.trim() || null,
      anoLancamento: Number(formulario.anoLancamento),
      preco: Number(formulario.preco),
      estoque: Number(formulario.estoque),
      idPlataforma: Number(formulario.idPlataforma),
      idCondicaoJogo: Number(formulario.idCondicaoJogo),
      idsGeneros: formulario.idsGeneros.map(Number),
    }

    try {
      if (estaEditando) {
        await jogoApi.alterar(id, {
          ...dados,
          ativo: formulario.ativo,
        })
      } else {
        await jogoApi.inserir(dados)
      }

      navigate('/admin/jogos')
    } catch (erro) {
      setMensagemErro(obterMensagemErro(erro))
      setErrosCampos(obterErrosDosCampos(erro))
    } finally {
      setEstaSalvando(false)
    }
  }

  if (estaCarregando) {
    return <Carregando mensagem="Carregando formulário..." />
  }

  if (mensagemErro && estaEditando && !formulario.nome) {
    return (
      <MensagemErro
        mensagem={mensagemErro}
        aoTentarNovamente={() => window.location.reload()}
      />
    )
  }

  const erroGeneros = obterErroGeneros()

  return (
    <div>
      <div className="mb-4">
        <h1>{estaEditando ? 'Editar jogo' : 'Novo jogo'}</h1>

        <p className="text-secondary">
          {estaEditando
            ? 'Altere as informações do jogo.'
            : 'Preencha as informações para cadastrar um novo jogo.'}
        </p>
      </div>

      {mensagemErro && (
        <div className="alert alert-danger" role="alert">
          {mensagemErro}
        </div>
      )}

      <form onSubmit={salvar}>
        <div className="card card-body mb-4">
          <div className="mb-3">
            <label htmlFor="nome" className="form-label">
              Nome
            </label>

            <input
              id="nome"
              name="nome"
              type="text"
              className={`form-control ${
                obterErroCampo('Nome') ? 'is-invalid' : ''
              }`}
              maxLength={150}
              required
              value={formulario.nome}
              onChange={atualizarCampo}
            />

            {obterErroCampo('Nome') && (
              <div className="invalid-feedback">
                {obterErroCampo('Nome')}
              </div>
            )}
          </div>

          <div className="mb-3">
            <label htmlFor="descricao" className="form-label">
              Descrição
            </label>

            <textarea
              id="descricao"
              name="descricao"
              className={`form-control ${
                obterErroCampo('Descricao') ? 'is-invalid' : ''
              }`}
              rows="4"
              maxLength={1000}
              value={formulario.descricao}
              onChange={atualizarCampo}
            />

            {obterErroCampo('Descricao') && (
              <div className="invalid-feedback">
                {obterErroCampo('Descricao')}
              </div>
            )}
          </div>

          <div className="row g-3">
            <div className="col-md-4">
              <label htmlFor="anoLancamento" className="form-label">
                Ano de lançamento
              </label>

              <input
                id="anoLancamento"
                name="anoLancamento"
                type="number"
                className={`form-control ${
                  obterErroCampo('AnoLancamento') ? 'is-invalid' : ''
                }`}
                min="1970"
                max="2100"
                required
                value={formulario.anoLancamento}
                onChange={atualizarCampo}
              />

              {obterErroCampo('AnoLancamento') && (
                <div className="invalid-feedback">
                  {obterErroCampo('AnoLancamento')}
                </div>
              )}
            </div>

            <div className="col-md-4">
              <label htmlFor="preco" className="form-label">
                Preço
              </label>

              <input
                id="preco"
                name="preco"
                type="number"
                className={`form-control ${
                  obterErroCampo('Preco') ? 'is-invalid' : ''
                }`}
                min="0.01"
                step="0.01"
                required
                value={formulario.preco}
                onChange={atualizarCampo}
              />

              {obterErroCampo('Preco') && (
                <div className="invalid-feedback">
                  {obterErroCampo('Preco')}
                </div>
              )}
            </div>

            <div className="col-md-4">
              <label htmlFor="estoque" className="form-label">
                Estoque
              </label>

              <input
                id="estoque"
                name="estoque"
                type="number"
                className={`form-control ${
                  obterErroCampo('Estoque') ? 'is-invalid' : ''
                }`}
                min="0"
                step="1"
                required
                value={formulario.estoque}
                onChange={atualizarCampo}
              />

              {obterErroCampo('Estoque') && (
                <div className="invalid-feedback">
                  {obterErroCampo('Estoque')}
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="card card-body mb-4">
          <div className="mb-3">
            <label htmlFor="idPlataforma" className="form-label">
              Plataforma
            </label>

            <select
              id="idPlataforma"
              name="idPlataforma"
              className={`form-select ${
                obterErroCampo('IdPlataforma') ? 'is-invalid' : ''
              }`}
              required
              value={formulario.idPlataforma}
              onChange={atualizarCampo}
            >
              <option value="">Selecione uma plataforma</option>

              {plataformas.map((plataforma) => (
                <option key={plataforma.id} value={plataforma.id}>
                  {plataforma.nome}
                </option>
              ))}
            </select>

            {obterErroCampo('IdPlataforma') && (
              <div className="invalid-feedback">
                {obterErroCampo('IdPlataforma')}
              </div>
            )}
          </div>

          <div className="mb-3">
            <label htmlFor="idCondicaoJogo" className="form-label">
              Condição do jogo
            </label>

            <select
              id="idCondicaoJogo"
              name="idCondicaoJogo"
              className={`form-select ${
                obterErroCampo('IdCondicaoJogo') ? 'is-invalid' : ''
              }`}
              required
              value={formulario.idCondicaoJogo}
              onChange={atualizarCampo}
            >
              <option value="">Selecione uma condição</option>

              {condicoes.map((condicao) => (
                <option key={condicao.id} value={condicao.id}>
                  {condicao.descricao}
                </option>
              ))}
            </select>

            {obterErroCampo('IdCondicaoJogo') && (
              <div className="invalid-feedback">
                {obterErroCampo('IdCondicaoJogo')}
              </div>
            )}
          </div>
        </div>

        <div className="card card-body mb-4">
          <fieldset>
            <legend className="fs-5">Gêneros</legend>

            <p className="text-secondary">
              Selecione pelo menos um gênero para o jogo.
            </p>

            <div className="row">
              {generos.map((genero) => (
                <div className="col-sm-6 col-md-4 mb-2" key={genero.id}>
                  <div className="form-check">
                    <input
                      id={`genero-${genero.id}`}
                      type="checkbox"
                      className="form-check-input"
                      checked={formulario.idsGeneros.includes(
                        String(genero.id)
                      )}
                      onChange={(evento) =>
                        atualizarGenero(genero.id, evento.target.checked)
                      }
                    />

                    <label
                      htmlFor={`genero-${genero.id}`}
                      className="form-check-label"
                    >
                      {genero.nome}
                    </label>
                  </div>
                </div>
              ))}
            </div>

            {erroGeneros && (
              <div className="text-danger small mt-2">
                {erroGeneros}
              </div>
            )}
          </fieldset>
        </div>

        {estaEditando && (
          <div className="card card-body mb-4">
            <div className="form-check">
              <input
                id="ativo"
                name="ativo"
                type="checkbox"
                className="form-check-input"
                checked={formulario.ativo}
                onChange={(evento) => {
                  setFormulario((atual) => ({
                    ...atual,
                    ativo: evento.target.checked,
                  }))
                }}
              />

              <label htmlFor="ativo" className="form-check-label">
                Jogo ativo
              </label>
            </div>
          </div>
        )}

        <div className="d-flex gap-2 mb-4">
          <button
            type="submit"
            className="btn btn-primary"
            disabled={estaSalvando}
          >
            {estaSalvando
              ? 'Salvando...'
              : estaEditando
                ? 'Salvar alterações'
                : 'Cadastrar jogo'}
          </button>

          <Link to="/admin/jogos" className="btn btn-outline-secondary">
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  )
}

export default FormularioJogo
