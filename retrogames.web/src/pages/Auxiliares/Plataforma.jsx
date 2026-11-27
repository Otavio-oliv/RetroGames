
import { useEffect, useState } from 'react'

import {
  Carregando,
  MensagemErro,
  EstadoVazio,
} from '../../components/MensagensEstado'

import { plataformaApi } from '../../services/servicos'

import {
  obterMensagemErro,
  obterErrosDosCampos,
} from '../../services/erros'

function Plataforma() {
  const [plataformas, setPlataformas] = useState([])

  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [sucesso, setSucesso] = useState('')

  const [tentativa, setTentativa] = useState(0)

  const [idEmEdicao, setIdEmEdicao] = useState(null)

  const [nome, setNome] = useState('')
  const [fabricante, setFabricante] = useState('')
  const [anoLancamento, setAnoLancamento] = useState('')

  const [errosCampos, setErrosCampos] = useState({})
  const [salvando, setSalvando] = useState(false)
  const [idExcluindo, setIdExcluindo] = useState(null)

  // Carrega a lista de plataformas.
  useEffect(() => {
    let ativo = true

    async function carregarPlataformas() {
      setCarregando(true)
      setErro('')

      try {
        const resultado = await plataformaApi.listar()

        if (ativo) {
          setPlataformas(resultado)
        }
      } catch (erro) {
        if (ativo) {
          setErro(obterMensagemErro(erro))
          setPlataformas([])
        }
      } finally {
        if (ativo) {
          setCarregando(false)
        }
      }
    }

    carregarPlataformas()

    return () => {
      ativo = false
    }
  }, [tentativa])

  // Limpa o formulário.
  function limparFormulario() {
    setIdEmEdicao(null)
    setNome('')
    setFabricante('')
    setAnoLancamento('')
    setErrosCampos({})
  }

  // Preenche o formulário com os dados da plataforma selecionada.
  function iniciarEdicao(plataforma) {
    setIdEmEdicao(plataforma.id)
    setNome(plataforma.nome)
    setFabricante(plataforma.fabricante)
    setAnoLancamento(String(plataforma.anoLancamento))
    setErrosCampos({})
    setErro('')
    setSucesso('')
  }

  // Cadastra ou altera uma plataforma.
  async function handleSubmit(event) {
    event.preventDefault()

    setErro('')
    setSucesso('')
    setErrosCampos({})
    setSalvando(true)

    const dados = {
      nome: nome.trim(),
      fabricante: fabricante.trim(),
      anoLancamento: Number(anoLancamento),
    }

    try {
      if (idEmEdicao !== null) {
        await plataformaApi.alterar(idEmEdicao, dados)
        setSucesso('Plataforma alterada com sucesso.')
      } else {
        await plataformaApi.inserir(dados)
        setSucesso('Plataforma cadastrada com sucesso.')
      }

      limparFormulario()
      setTentativa((valorAtual) => valorAtual + 1)
    } catch (erro) {
      const erros = obterErrosDosCampos(erro)

      if (Object.keys(erros).length > 0) {
        setErrosCampos(erros)
      } else {
        setErro(obterMensagemErro(erro))
      }
    } finally {
      setSalvando(false)
    }
  }

  // Exclui uma plataforma após confirmação.
  async function handleExcluir(plataforma) {
    const confirmou = window.confirm(
      `Deseja realmente excluir a plataforma "${plataforma.nome}"?`,
    )

    if (!confirmou) {
      return
    }

    setErro('')
    setSucesso('')
    setIdExcluindo(plataforma.id)

    try {
      await plataformaApi.excluir(plataforma.id)

      setSucesso('Plataforma excluída com sucesso.')

      if (idEmEdicao === plataforma.id) {
        limparFormulario()
      }

      setTentativa((valorAtual) => valorAtual + 1)
    } catch (erro) {
      setErro(obterMensagemErro(erro))
    } finally {
      setIdExcluindo(null)
    }
  }

  // Recupera uma mensagem de validação do campo.
  function obterErroCampo(nomeCampo, nomeAlternativo) {
    return (
      errosCampos[nomeCampo] ||
      errosCampos[nomeAlternativo] ||
      ''
    )
  }

  const erroNome = obterErroCampo('nome', 'Nome')
  const erroFabricante = obterErroCampo('fabricante', 'Fabricante')
  const erroAno = obterErroCampo('anoLancamento', 'AnoLancamento')

  return (
    <div>
      <div className="mb-4">
        <h1 className="h2 fw-bold mb-2">
          Administração de plataformas
        </h1>

        <p className="text-secondary">
          Cadastre, edite e exclua as plataformas de jogos.
        </p>
      </div>

      {erro && (
        <MensagemErro mensagem={erro} />
      )}

      {sucesso && (
        <div className="alert alert-success" role="status">
          {sucesso}
        </div>
      )}

      {/* Formulário de cadastro e edição */}
      <div className="card shadow-sm mb-4">
        <div className="card-body p-4">
          <h2 className="h5 fw-bold mb-3">
            {idEmEdicao !== null
              ? 'Editar plataforma'
              : 'Nova plataforma'}
          </h2>

          <form onSubmit={handleSubmit}>
            <div className="row g-3">
              <div className="col-12 col-md-5">
                <label htmlFor="nome" className="form-label">
                  Nome
                </label>

                <input
                  id="nome"
                  type="text"
                  className={`form-control ${erroNome ? 'is-invalid' : ''}`}
                  value={nome}
                  onChange={(event) => setNome(event.target.value)}
                  maxLength={80}
                  required
                  disabled={salvando}
                />

                {erroNome && (
                  <div className="invalid-feedback">
                    {erroNome}
                  </div>
                )}
              </div>

              <div className="col-12 col-md-4">
                <label htmlFor="fabricante" className="form-label">
                  Fabricante
                </label>

                <input
                  id="fabricante"
                  type="text"
                  className={`form-control ${erroFabricante ? 'is-invalid' : ''}`}
                  value={fabricante}
                  onChange={(event) => setFabricante(event.target.value)}
                  maxLength={80}
                  required
                  disabled={salvando}
                />

                {erroFabricante && (
                  <div className="invalid-feedback">
                    {erroFabricante}
                  </div>
                )}
              </div>

              <div className="col-12 col-md-3">
                <label htmlFor="anoLancamento" className="form-label">
                  Ano de lançamento
                </label>

                <input
                  id="anoLancamento"
                  type="number"
                  className={`form-control ${erroAno ? 'is-invalid' : ''}`}
                  value={anoLancamento}
                  onChange={(event) => setAnoLancamento(event.target.value)}
                  min={1970}
                  max={2100}
                  required
                  disabled={salvando}
                />

                {erroAno && (
                  <div className="invalid-feedback">
                    {erroAno}
                  </div>
                )}
              </div>
            </div>

            <div className="d-flex gap-2 mt-4">
              <button
                type="submit"
                className="btn btn-primary"
                disabled={salvando}
              >
                {salvando
                  ? 'Salvando...'
                  : idEmEdicao !== null
                    ? 'Salvar alterações'
                    : 'Cadastrar plataforma'}
              </button>

              {idEmEdicao !== null && (
                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={limparFormulario}
                  disabled={salvando}
                >
                  Cancelar edição
                </button>
              )}
            </div>
          </form>
        </div>
      </div>

      {/* Lista de plataformas */}
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="h5 fw-bold mb-0">
          Plataformas cadastradas
        </h2>

        {!carregando && !erro && (
          <span className="text-secondary small">
            {plataformas.length}{' '}
            {plataformas.length === 1
              ? 'plataforma'
              : 'plataformas'}
          </span>
        )}
      </div>

      {carregando ? (
        <Carregando mensagem="Carregando plataformas..." />
      ) : erro && plataformas.length === 0 ? (
        <MensagemErro
          mensagem={erro}
          aoTentarNovamente={() =>
            setTentativa((valorAtual) => valorAtual + 1)
          }
        />
      ) : plataformas.length === 0 ? (
        <EstadoVazio mensagem="Nenhuma plataforma cadastrada." />
      ) : (
        <div className="card shadow-sm">
          <div className="table-responsive">
            <table className="table table-hover align-middle mb-0">
              <thead className="table-light">
                <tr>
                  <th>Nome</th>
                  <th>Fabricante</th>
                  <th>Ano de lançamento</th>
                  <th className="text-end">Ações</th>
                </tr>
              </thead>

              <tbody>
                {plataformas.map((plataforma) => (
                  <tr key={plataforma.id}>
                    <td className="fw-semibold">
                      {plataforma.nome}
                    </td>

                    <td>{plataforma.fabricante}</td>

                    <td>{plataforma.anoLancamento}</td>

                    <td>
                      <div className="d-flex justify-content-end gap-2">
                        <button
                          type="button"
                          className="btn btn-sm btn-outline-primary"
                          onClick={() => iniciarEdicao(plataforma)}
                          disabled={idExcluindo !== null}
                        >
                          Editar
                        </button>

                        <button
                          type="button"
                          className="btn btn-sm btn-outline-danger"
                          onClick={() => handleExcluir(plataforma)}
                          disabled={idExcluindo !== null}
                        >
                          {idExcluindo === plataforma.id
                            ? 'Excluindo...'
                            : 'Excluir'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}

export default Plataforma
