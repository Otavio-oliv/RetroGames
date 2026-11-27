
import { Link } from 'react-router-dom'
import { formatarPreco } from '../lib/formato'

function CardJogo({ jogo }) {
  return (
    <div className="card h-100 shadow-sm">
      <div className="card-body d-flex flex-column">
        <div className="d-flex justify-content-between align-items-start gap-2 mb-2">
          <h5 className="card-title mb-0">{jogo.nome}</h5>

          {jogo.ativo ? (
            <span className="badge text-bg-success">Ativo</span>
          ) : (
            <span className="badge text-bg-secondary">Inativo</span>
          )}
        </div>

        <p className="text-secondary mb-2">
          {jogo.nomePlataforma} · {jogo.anoLancamento}
        </p>

        <p className="mb-2">
          <strong>Condição:</strong> {jogo.descricaoCondicao}
        </p>

        {jogo.generos?.length > 0 && (
          <div className="mb-3">
            {jogo.generos.map((genero) => (
              <span
                key={genero}
                className="badge text-bg-light border text-dark me-1 mb-1"
              >
                {genero}
              </span>
            ))}
          </div>
        )}

        <div className="mt-auto">
          <p className="fs-5 fw-bold text-primary mb-1">
            {formatarPreco(jogo.preco)}
          </p>

          {jogo.estoque > 0 ? (
            <p className="text-secondary small mb-3">
              Estoque: {jogo.estoque}
            </p>
          ) : (
            <p className="text-danger fw-bold small mb-3">
              <span className="badge text-bg-danger">Esgotado</span>
              {' '}Sem unidades disponíveis
            </p>
          )}

          <Link
            to={`/jogos/${jogo.id}`}
            className="btn btn-outline-primary w-100"
          >
            Ver detalhes
          </Link>
        </div>
      </div>
    </div>
  )
}

export default CardJogo
