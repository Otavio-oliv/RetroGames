import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'

import { useCarrinho } from '../contexts/CarrinhoContext'
import { formatarPreco } from '../lib/formato'
import { pedidoApi } from '../services/servicos'
import { obterMensagemErro } from '../services/erros'

function Carrinho() {
  const navigate = useNavigate()

  const {
    itens,
    quantidadeTotal,
    valorTotal,
    alterarQuantidade,
    removerItem,
    limparCarrinho,
  } = useCarrinho()

  const [finalizando, setFinalizando] = useState(false)
  const [erro, setErro] = useState('')

  async function finalizarCompra() {
    setErro('')
    setFinalizando(true)

    try {
      const itensPedido = itens.map((item) => ({
        idJogo: item.id,
        quantidade: item.quantidade,
      }))

      const pedido = await pedidoApi.criar(itensPedido)

      limparCarrinho()

      navigate('/meus-pedidos', {
        replace: true,
        state: {
          pedidoCriado: true,
          idPedido: pedido.id,
        },
      })
    } catch (erro) {
      setErro(obterMensagemErro(erro))
    } finally {
      setFinalizando(false)
    }
  }

  if (itens.length === 0) {
    return (
      <div>
        <h1 className="h2 fw-bold mb-4">Meu carrinho</h1>

        <div className="card shadow-sm">
          <div className="card-body text-center py-5">
            <h2 className="h5 mb-3">
              Seu carrinho está vazio.
            </h2>

            <p className="text-secondary">
              Explore nosso catálogo e encontre seus jogos favoritos.
            </p>

            <Link to="/" className="btn btn-primary">
              Explorar catálogo
            </Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div>
      <div className="mb-4">
        <h1 className="h2 fw-bold mb-2">Meu carrinho</h1>

        <p className="text-secondary">
          Confira os jogos selecionados antes de finalizar a compra.
        </p>
      </div>

      {erro && (
        <div className="alert alert-danger" role="alert">
          {erro}
        </div>
      )}

      <div className="row g-4">
        <div className="col-12 col-lg-8">
          <div className="d-flex flex-column gap-3">
            {itens.map((item) => {
              const limite = Math.min(10, item.estoque)

              return (
                <div className="card shadow-sm" key={item.id}>
                  <div className="card-body">
                    <div className="row align-items-center g-3">
                      <div className="col-12 col-md">
                        <h2 className="h5 fw-bold mb-1">
                          {item.nome}
                        </h2>

                        {item.nomePlataforma && (
                          <p className="text-secondary small mb-2">
                            {item.nomePlataforma}
                          </p>
                        )}

                        <p className="mb-0">
                          <span className="text-secondary">
                            Preço unitário:
                          </span>{' '}
                          <strong>{formatarPreco(item.preco)}</strong>
                        </p>
                      </div>

                      <div className="col-7 col-md-auto">
                        <label
                          className="form-label small mb-1"
                          htmlFor={`quantidade-${item.id}`}
                        >
                          Quantidade
                        </label>

                        <div className="input-group">
                          <button
                            type="button"
                            className="btn btn-outline-secondary"
                            aria-label={`Diminuir quantidade de ${item.nome}`}
                            disabled={
                              finalizando || item.quantidade <= 1
                            }
                            onClick={() =>
                              alterarQuantidade(
                                item.id,
                                item.quantidade - 1,
                              )
                            }
                          >
                            −
                          </button>

                          <input
                            id={`quantidade-${item.id}`}
                            className="form-control text-center"
                            type="number"
                            value={item.quantidade}
                            readOnly
                            aria-label={`Quantidade de ${item.nome}`}
                          />

                          <button
                            type="button"
                            className="btn btn-outline-secondary"
                            disabled={
                              finalizando || item.quantidade >= limite
                            }
                            aria-label={`Aumentar quantidade de ${item.nome}`}
                            onClick={() =>
                              alterarQuantidade(
                                item.id,
                                item.quantidade + 1,
                              )
                            }
                          >
                            +
                          </button>
                        </div>

                        <small className="text-secondary">
                          Limite: {limite}
                        </small>
                      </div>

                      <div className="col-5 col-md-auto text-md-end">
                        <p className="text-secondary small mb-1">
                          Subtotal
                        </p>

                        <p className="fw-bold mb-2">
                          {formatarPreco(
                            item.preco * item.quantidade,
                          )}
                        </p>

                        <button
                          type="button"
                          className="btn btn-sm btn-outline-danger"
                          disabled={finalizando}
                          onClick={() => removerItem(item.id)}
                        >
                          Remover
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        </div>

        <div className="col-12 col-lg-4">
          <div className="card shadow-sm">
            <div className="card-body">
              <h2 className="h5 fw-bold mb-3">
                Resumo da compra
              </h2>

              <div className="d-flex justify-content-between mb-2">
                <span className="text-secondary">
                  Quantidade de itens
                </span>

                <strong>{quantidadeTotal}</strong>
              </div>

              <hr />

              <div className="d-flex justify-content-between align-items-center">
                <span className="fw-bold">Total estimado</span>

                <span className="fs-4 fw-bold text-primary">
                  {formatarPreco(valorTotal)}
                </span>
              </div>

              <p className="text-secondary small mt-3 mb-3">
                O valor e a disponibilidade serão conferidos pelo
                servidor antes da confirmação do pedido.
              </p>

              <button
                type="button"
                className="btn btn-primary w-100"
                disabled={finalizando || itens.length === 0}
                onClick={finalizarCompra}
              >
                {finalizando
                  ? 'Finalizando compra...'
                  : 'Finalizar compra'}
              </button>
            </div>
          </div>

          <Link
            to="/"
            className="btn btn-outline-primary w-100 mt-3"
          >
            Continuar comprando
          </Link>
        </div>
      </div>
    </div>
  )
}

export default Carrinho
