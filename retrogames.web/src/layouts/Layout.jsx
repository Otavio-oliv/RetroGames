import { Link, Outlet, useNavigate } from 'react-router-dom'

import { useAutenticacao } from '../contexts/AutenticacaoContext'
import { useCarrinho } from '../contexts/CarrinhoContext'

function Layout() {
  const navigate = useNavigate()

  const {
    usuario,
    estaCarregando,
    estaAutenticado,
    ehAdministrador,
    sair,
  } = useAutenticacao()

  const { quantidadeTotal } = useCarrinho()

  const ehCliente = usuario?.perfil === 'Cliente'

  async function handleSair() {
    try {
      await sair()
      navigate('/', { replace: true })
    } catch {
      navigate('/', { replace: true })
    }
  }

  return (
    <div className="d-flex flex-column min-vh-100">
      <header>
        <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
          <div className="container">
            <Link className="navbar-brand fw-bold" to="/">
              RetroGames
            </Link>

            <button
              className="navbar-toggler"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target="#menuPrincipal"
              aria-controls="menuPrincipal"
              aria-expanded="false"
              aria-label="Alternar navegação"
            >
              <span className="navbar-toggler-icon" />
            </button>

            <div className="collapse navbar-collapse" id="menuPrincipal">
              <ul className="navbar-nav ms-auto align-items-lg-center">
                <li className="nav-item">
                  <Link className="nav-link" to="/">
                    Catálogo
                  </Link>
                </li>

                {!estaCarregando && !estaAutenticado && (
                  <>
                    <li className="nav-item">
                      <Link className="nav-link" to="/cadastro">
                        Cadastrar-se
                      </Link>
                    </li>

                    <li className="nav-item">
                      <Link className="nav-link" to="/login">
                        Entrar
                      </Link>
                    </li>
                  </>
                )}

                {!estaCarregando && estaAutenticado && (
                  <>
                    {ehCliente && (
                      <>
                        <li className="nav-item">
                          <Link className="nav-link" to="/carrinho">
                            Carrinho
                            {quantidadeTotal > 0 && (
                              <span className="badge rounded-pill text-bg-primary ms-1">
                                {quantidadeTotal}
                              </span>
                            )}
                          </Link>
                        </li>

                        <li className="nav-item">
                          <Link className="nav-link" to="/meus-pedidos">
                            Meus pedidos
                          </Link>
                        </li>
                      </>
                    )}

                    {ehAdministrador && (
                      <li className="nav-item dropdown">
                        <button
                          className="nav-link dropdown-toggle btn btn-link"
                          type="button"
                          data-bs-toggle="dropdown"
                          aria-expanded="false"
                        >
                          Administração
                        </button>

                        <ul className="dropdown-menu dropdown-menu-end">
                          <li>
                            <Link
                              className="dropdown-item"
                              to="/admin/jogos"
                            >
                              Jogos
                            </Link>
                          </li>

                          <li>
                            <Link
                              className="dropdown-item"
                              to="/admin/plataformas"
                            >
                              Plataformas
                            </Link>
                          </li>
                        </ul>
                      </li>
                    )}

                    <li className="nav-item">
                      <span className="nav-link">
                        Olá, {usuario.nome}
                      </span>
                    </li>

                    <li className="nav-item">
                      <button
                        type="button"
                        className="nav-link btn btn-link"
                        onClick={handleSair}
                      >
                        Sair
                      </button>
                    </li>
                  </>
                )}
              </ul>
            </div>
          </div>
        </nav>
      </header>

      <main className="container flex-grow-1 py-4">
        <Outlet />
      </main>

      <footer className="bg-dark text-light text-center py-3 mt-auto">
        <div className="container">
          <small>RetroGames - Catálogo de jogos clássicos</small>
        </div>
      </footer>
    </div>
  )
}

export default Layout
