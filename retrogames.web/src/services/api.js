
import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
})

api.interceptors.response.use(
  (response) => response,
  (erro) => {
    if (erro.response?.status === 401) {
      const url = erro.config?.url ?? ''

      const ehConsultaDeSessao = url.includes('/Autenticacao/eu')
      const ehLogin = url.includes('/Autenticacao/login')

      if (!ehConsultaDeSessao && !ehLogin) {
        window.dispatchEvent(new Event('sessao:expirada'))
      }
    }

    return Promise.reject(erro)
  },
)

export default api
