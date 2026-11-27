
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

  async inserir(dados) {
    const response = await api.post('/Jogo', dados)
    return response.data.data
  },

  async alterar(id, dados) {
    const response = await api.put(`/Jogo/${id}`, dados)
    return response.data.data
  },

  async desativar(id) {
    const response = await api.delete(`/Jogo/${id}`)
    return response.data.data
  },
}

export const plataformaApi = {
  async listar() {
    const response = await api.get('/Plataforma')
    return response.data.data
  },

  async buscarPorId(id) {
    const response = await api.get(`/Plataforma/${id}`)
    return response.data.data
  },

  async inserir(dados) {
    const response = await api.post('/Plataforma', dados)
    return response.data.data
  },

  async alterar(id, dados) {
    const response = await api.put(`/Plataforma/${id}`, dados)
    return response.data.data
  },

  async excluir(id) {
    const response = await api.delete(`/Plataforma/${id}`)
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
