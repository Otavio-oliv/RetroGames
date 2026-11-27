import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'

import { useAutenticacao } from '../contexts/AutenticacaoContext'
import { obterMensagemErro } from '../services/erros'

function Login() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()

  const { entrar } = useAutenticacao()

  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState('')
  const [carregando, setCarregando] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()

    setErro('')
    setCarregando(true)

    try {
      await entrar(email, senha)

      const voltarPara = searchParams.get('voltarPara')

      navigate(
        voltarPara?.startsWith('/') ? voltarPara : '/',
        { replace: true },
      )
    } catch (erro) {
      setErro(obterMensagemErro(erro))
    } finally {
      setCarregando(false)
    }
  }

  return (
    <div className="row justify-content-center">
      <div className="col-12 col-md-7 col-lg-5">
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h1 className="h3 fw-bold text-center mb-2">
              Entrar
            </h1>

            <p className="text-secondary text-center mb-4">
              Acesse sua conta RetroGames.
            </p>

            {erro && (
              <div className="alert alert-danger" role="alert">
                {erro}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label htmlFor="email" className="form-label">
                  E-mail
                </label>

                <input
                  id="email"
                  type="email"
                  className="form-control"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  autoComplete="username"
                  required
                  disabled={carregando}
                />
              </div>

              <div className="mb-4">
                <label htmlFor="senha" className="form-label">
                  Senha
                </label>

                <input
                  id="senha"
                  type="password"
                  className="form-control"
                  value={senha}
                  onChange={(event) => setSenha(event.target.value)}
                  autoComplete="current-password"
                  required
                  disabled={carregando}
                />
              </div>

              <button
                type="submit"
                className="btn btn-primary w-100"
                disabled={carregando}
              >
                {carregando ? 'Entrando...' : 'Entrar'}
              </button>
            </form>

            <div className="text-center mt-3">
              <span className="text-secondary">
                Ainda não possui uma conta?{' '}
              </span>

              <Link to="/cadastro" className="text-decoration-none">
                Cadastre-se
              </Link>
            </div>

            <div className="text-center mt-2">
              <Link to="/" className="text-decoration-none">
                Voltar ao catálogo
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Login
