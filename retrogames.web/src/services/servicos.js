
import api from './api'

export const autenticacaoApi = {
  async entrar(email, senha) {
    const response = await api.post('/Autenticacao/login', {
      email,
      senha,
    })

    return response.data.data
  },

  async sair() {
    await api.post('/Autenticacao/logout')
  },

  async obterUsuarioAtual() {
    const response = await api.get('/Autenticacao/eu')
    return response.data.data
  },
}

export const jogoApi = {
  async listar(params = {}) {
    const response = await api.get('/Jogo', { params })
    return response.data.data
  },

  async buscarPorId(id) {
    const response = await api.get(`/Jogo/${id}`)
    return response.data.data
  },
}

export const plataformaApi = {
  async listar() {
    const response = await api.get('/Plataforma')
    return response.data.data
  },
}

export const generoApi = {
  async listar() {
    const response = await api.get('/Genero')
    return response.data.data
  },
}

export const condicaoJogoApi = {
  async listar() {
    const response = await api.get('/CondicaoJogo')
    return response.data.data
  },
}
