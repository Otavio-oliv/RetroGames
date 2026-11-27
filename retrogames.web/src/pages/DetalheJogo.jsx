
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'

import {
  Carregando,
  MensagemErro,
} from '../components/MensagensEstado'

import { jogoApi } from '../services/servicos'
import { obterMensagemErro } from '../services/erros'
import { formatarPreco } from '../lib/formato'

function DetalheJogo() {
  const { id } = useParams()

  const [jogo, setJogo] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')

  useEffect(() => {
    let ativo = true

    async function carregarJogo() {
      setCarregando(true)
      setErro('')
      setJogo(null)

      try {
        const resultado = await jogoApi.buscarPorId(id)

        if (ativo) {
          setJogo(resultado)
        }
      } catch (erro) {
        if (ativo) {
          setErro(obterMensagemErro(erro))
        }
      } finally {
        if (ativo) {
          setCarregando(false)
        }
      }
    }

    carregarJogo()

    return () => {
      ativo = false
    }
  }, [id])

  if (carregando) {
    return <Carregando mensagem="Carregando detalhes do jogo..." />
  }

  if (erro) {
    return (
      <div>
        <MensagemErro mensagem={erro} />

        <Link to="/" className="btn btn-outline-primary mt-3">
          Voltar ao catálogo
        </Link>
      </div>
    )
  }

  if (!jogo) {
    return null
  }

  return (
    <div>
      <Link to="/" className="btn btn-outline-secondary mb-4">
        &larr; Voltar ao catálogo
      </Link>

      <div className="card shadow-sm">
        <div className="card-body p-4">
          <div className="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-3">
            <h1 className="h2 fw-bold mb-0">{jogo.nome}</h1>

            {jogo.ativo ? (
              <span className="badge text-bg-success">Ativo</span>
            ) : (
              <span className="badge text-bg-secondary">Inativo</span>
            )}
          </div>

          <p className="text-secondary">
            {jogo.nomePlataforma} · {jogo.anoLancamento}
          </p>

          <hr />

          <h2 className="h5">Descrição</h2>

          {jogo.descricao ? (
            <p>{jogo.descricao}</p>
          ) : (
            <p className="text-secondary">Descrição não disponível.</p>
          )}

          <hr />

          <h2 className="h5">Informações do jogo</h2>

          <p>
            <strong>Plataforma:</strong> {jogo.nomePlataforma}
          </p>

          <p>
            <strong>Condição:</strong> {jogo.descricaoCondicao}
          </p>

          <p>
            <strong>Ano de lançamento:</strong> {jogo.anoLancamento}
          </p>

          <div className="mb-3">
            <strong>Gêneros:</strong>

            <div className="mt-2">
              {jogo.generos?.length > 0 ? (
                jogo.generos.map((genero) => (
                  <span
                    key={genero.id}
                    className="badge text-bg-light border text-dark me-2 mb-2"
                  >
                    {genero.nome}
                  </span>
                ))
              ) : (
                <span className="text-secondary">
                  Nenhum gênero informado.
                </span>
              )}
            </div>
          </div>

          <hr />

          <div className="row g-3">
            <div className="col-12 col-md-6">
              <p className="text-secondary mb-1">Preço</p>

              <p className="fs-3 fw-bold text-primary">
                {formatarPreco(jogo.preco)}
              </p>
            </div>

            <div className="col-12 col-md-6">
              <p className="text-secondary mb-1">Estoque</p>

              <p className="fs-4 fw-bold">
                {jogo.estoque} unidades
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default DetalheJogo
