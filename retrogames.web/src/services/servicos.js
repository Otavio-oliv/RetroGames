import api from './api'

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
