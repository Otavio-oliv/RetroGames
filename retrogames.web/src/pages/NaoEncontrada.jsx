import { Link } from 'react-router-dom'

function NaoEncontrada() {
  return (
    <div className="text-center py-5">
      <h1 className="display-1 fw-bold">404</h1>

      <h2 className="mb-3">Página não encontrada</h2>

      <p className="text-secondary mb-4">
        A página que você está procurando não existe.
      </p>

      <Link to="/" className="btn btn-primary">
        Voltar ao catálogo
      </Link>
    </div>
  )
}

export default NaoEncontrada
