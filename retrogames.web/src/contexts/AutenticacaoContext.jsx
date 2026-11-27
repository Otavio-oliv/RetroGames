
import {
  createContext,
  useContext,
  useEffect,
  useState,
} from 'react'

import { autenticacaoApi } from '../services/servicos'

const AutenticacaoContext = createContext(null)

export function AutenticacaoProvider({ children }) {
  const [usuario, setUsuario] = useState(null)
  const [estaCarregando, setEstaCarregando] = useState(true)

  useEffect(() => {
    let componenteAtivo = true

    async function carregarUsuario() {
      try {
        const usuarioAtual =
          await autenticacaoApi.obterUsuarioAtual()

        if (componenteAtivo) {
          setUsuario(usuarioAtual)
        }
      } catch {
        if (componenteAtivo) {
          setUsuario(null)
        }
      } finally {
        if (componenteAtivo) {
          setEstaCarregando(false)
        }
      }
    }

    function tratarSessaoExpirada() {
      setUsuario(null)
    }

    window.addEventListener(
      'sessao:expirada',
      tratarSessaoExpirada,
    )

    carregarUsuario()

    return () => {
      componenteAtivo = false

      window.removeEventListener(
        'sessao:expirada',
        tratarSessaoExpirada,
      )
    }
  }, [])

  async function entrar(email, senha) {
    const usuarioAutenticado =
      await autenticacaoApi.entrar(email, senha)

    setUsuario(usuarioAutenticado)

    return usuarioAutenticado
  }

  async function sair() {
    try {
      await autenticacaoApi.sair()
    } finally {
      setUsuario(null)
    }
  }

  const estaAutenticado = usuario !== null

  const ehAdministrador =
    usuario?.perfil === 'Administrador'

  const valor = {
    usuario,
    estaCarregando,
    estaAutenticado,
    ehAdministrador,
    entrar,
    sair,
  }

  return (
    <AutenticacaoContext.Provider value={valor}>
      {children}
    </AutenticacaoContext.Provider>
  )
}

export function useAutenticacao() {
  const contexto = useContext(AutenticacaoContext)

  if (!contexto) {
    throw new Error(
      'useAutenticacao deve ser utilizado dentro de AutenticacaoProvider.',
    )
  }

  return contexto
}
