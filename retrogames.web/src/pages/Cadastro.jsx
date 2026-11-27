import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'

import { useAutenticacao } from '../contexts/AutenticacaoContext'
import { obterMensagemErro } from '../services/erros'

function Cadastro() {
  const navigate = useNavigate()
  const { cadastrar } = useAutenticacao()

  const [nome, setNome] = useState('')
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [confirmarSenha, setConfirmarSenha] = useState('')
  const [erro, setErro] = useState('')
  const [carregando, setCarregando] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()

    setErro('')

    if (senha !== confirmarSenha) {
      setErro('As senhas não coincidem.')
      return
    }

    setCarregando(true)

    try {
      await cadastrar(nome, email, senha)

      navigate('/', { replace: true })
    } catch (erro) {
      setErro(obterMensagemErro(erro))
    } finally {
      setCarregando(false)
    }
  }

  return (
    <div className="row justify-content-center">
      <div className="col-12 col-md-8 col-lg-6">
        <div className="card shadow-sm">
          <div className="card-body p-4">
            <h1 className="h3 fw-bold text-center mb-2">
              Criar conta
            </h1>

            <p className="text-secondary text-center mb-4">
              Cadastre-se para aproveitar a RetroGames.
            </p>

            {erro && (
              <div className="alert alert-danger" role="alert">
                {erro}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label htmlFor="nome" className="form-label">
                  Nome
                </label>

                <input
                  id="nome"
                  type="text"
                  className="form-control"
                  value={nome}
                  onChange={(event) => setNome(event.target.value)}
                  autoComplete="name"
                  maxLength={100}
                  required
                  disabled={carregando}
                />
              </div>

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
                  autoComplete="email"
                  maxLength={150}
                  required
                  disabled={carregando}
                />
              </div>

              <div className="mb-3">
                <label htmlFor="senha" className="form-label">
                  Senha
                </label>

                <input
                  id="senha"
                  type="password"
                  className="form-control"
                  value={senha}
                  onChange={(event) => setSenha(event.target.value)}
                  autoComplete="new-password"
                  required
                  disabled={carregando}
                />
              </div>

              <div className="mb-4">
                <label
                  htmlFor="confirmarSenha"
                  className="form-label"
                >
                  Confirmar senha
                </label>

                <input
                  id="confirmarSenha"
                  type="password"
                  className="form-control"
                  value={confirmarSenha}
                  onChange={(event) =>
                    setConfirmarSenha(event.target.value)
                  }
                  autoComplete="new-password"
                  required
                  disabled={carregando}
                />
              </div>

              <button
                type="submit"
                className="btn btn-primary w-100"
                disabled={carregando}
              >
                {carregando ? 'Cadastrando...' : 'Criar conta'}
              </button>
            </form>

            <div className="text-center mt-3">
              <span className="text-secondary">
                Já possui uma conta?{' '}
              </span>

              <Link to="/login" className="text-decoration-none">
                Entrar
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Cadastro
