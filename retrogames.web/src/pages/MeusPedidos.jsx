import { useEffect, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'

import { formatarDataHora, formatarPreco } from '../lib/formato'
import { pedidoApi } from '../services/servicos'
import { obterMensagemErro } from '../services/erros'

function MeusPedidos() {
  const location = useLocation()

  const [pedidos, setPedidos] = useState([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')

  useEffect(() => {
    let componenteAtivo = true

    async function carregarPedidos() {
      setCarregando(true)
      setErro('')

      try {
        const dados = await pedidoApi.listarMeus()

        if (componenteAtivo) {
          setPedidos(Array.isArray(dados) ? dados : [])
        }
      } catch (erro) {
        if (componenteAtivo) {
          setErro(obterMensagemErro(erro))
        }
      } finally {
        if (componenteAtivo) {
          setCarregando(false)
        }
      }
    }

    carregarPedidos()

    return () => {
      componenteAtivo = false
    }
  }, [])

  return (
    <div>
      <div className="mb-4">
        <h1 className="h2 fw-bold mb-2">Meus pedidos</h1>

        <p className="text-secondary">
          Consulte suas compras e acompanhe a situação de cada pedido.
        </p>
      </div>

      {location.state?.pedidoCriado && (
        <div className="alert alert-success" role="alert">
          Pedido #{location.state.idPedido} criado com sucesso!
        </div>
      )}

      {erro && (
        <div className="alert alert-danger" role="alert">
          {erro}
        </div>
      )}

      {carregando && (
        <div className="text-center py-5">
          <div
            className="spinner-border text-primary"
            role="status"
          >
            <span className="visually-hidden">
              Carregando pedidos...
            </span>
          </div>

          <p className="text-secondary mt-3">
            Carregando seus pedidos...
          </p>
        </div>
      )}

      {!carregando && !erro && pedidos.length === 0 && (
        <div className="card shadow-sm">
          <div className="card-body text-center py-5">
            <h2 className="h5 mb-3">
              Você ainda não possui pedidos.
            </h2>

            <p className="text-secondary">
              Explore o catálogo e encontre seus jogos favoritos.
            </p>

            <Link to="/" className="btn btn-primary">
              Explorar catálogo
            </Link>
          </div>
        </div>
      )}

      {!carregando && !erro && pedidos.length > 0 && (
        <div className="d-flex flex-column gap-4">
          {pedidos.map((pedido) => (
            <div className="card shadow-sm" key={pedido.id}>
              <div className="card-body">
                <div className="d-flex flex-column flex-md-row justify-content-between gap-3 mb-3">
                  <div>
                    <h2 className="h5 fw-bold mb-1">
                      Pedido #{pedido.id}
                    </h2>

                    <p className="text-secondary small mb-0">
                      {formatarDataHora(pedido.dataPedido)}
                    </p>
                  </div>

                  <div className="text-md-end">
                    <span className="badge text-bg-primary">
                      {pedido.situacao}
                    </span>

                    <p className="fw-bold fs-5 mt-2 mb-0">
                      {formatarPreco(pedido.total)}
                    </p>
                  </div>
                </div>

                <hr />

                <h3 className="h6 fw-bold mb-3">
                  Itens do pedido
                </h3>

                <div className="d-flex flex-column gap-3">
                  {pedido.itens?.map((item, indice) => (
                    <div
                      className="d-flex justify-content-between align-items-start gap-3"
                      key={`${pedido.id}-${item.idJogo}-${indice}`}
                    >
                      <div>
                        <p className="fw-semibold mb-1">
                          {item.nomeJogo}
                        </p>

                        <p className="text-secondary small mb-0">
                          Quantidade: {item.quantidade}
                        </p>

                        <p className="text-secondary small mb-0">
                          Valor unitário:{' '}
                          {formatarPreco(item.valorUnitario)}
                        </p>
                      </div>

                      <strong className="text-nowrap">
                        {formatarPreco(
                          item.valorUnitario * item.quantidade,
                        )}
                      </strong>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="mt-4">
        <Link to="/" className="btn btn-outline-primary">
          Voltar ao catálogo
        </Link>
      </div>
    </div>
  )
}

export default MeusPedidos
