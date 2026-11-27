
import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
})

const metodosQueExigemCsrf = new Set([
  'post',
  'put',
  'patch',
  'delete',
])

let tokenCsrf = null
let requisicaoTokenCsrf = null

async function obterTokenCsrf() {
  if (tokenCsrf) {
    return tokenCsrf
  }

  if (!requisicaoTokenCsrf) {
    requisicaoTokenCsrf = api
      .get('/Autenticacao/csrf-token')
      .then((response) => {
        tokenCsrf = response.data.data.token

        return tokenCsrf
      })
      .finally(() => {
        requisicaoTokenCsrf = null
      })
  }

  return requisicaoTokenCsrf
}

function invalidarTokenCsrf() {
  tokenCsrf = null
  requisicaoTokenCsrf = null
}

api.interceptors.request.use(async (config) => {
  const metodo = (config.method ?? 'get').toLowerCase()

  if (metodosQueExigemCsrf.has(metodo)) {
    const token = await obterTokenCsrf()

    if (typeof config.headers.set === 'function') {
      config.headers.set('X-CSRF-TOKEN', token)
    } else {
      config.headers['X-CSRF-TOKEN'] = token
    }
  }

  return config
})

api.interceptors.response.use(
  (response) => {
    const url = response.config?.url ?? ''

    const houveMudancaDeSessao =
      url.includes('/Autenticacao/login') ||
      url.includes('/Autenticacao/cadastrar') ||
      url.includes('/Autenticacao/logout')

    if (houveMudancaDeSessao) {
      invalidarTokenCsrf()
    }

    return response
  },
  (erro) => {
    if (erro.response?.status === 401) {
      invalidarTokenCsrf()

      const url = erro.config?.url ?? ''

      const ehConsultaDeSessao =
        url.includes('/Autenticacao/eu')

      const ehLogin =
        url.includes('/Autenticacao/login')

      const ehCadastro =
        url.includes('/Autenticacao/cadastrar')

      if (
        !ehConsultaDeSessao &&
        !ehLogin &&
        !ehCadastro
      ) {
        window.dispatchEvent(new Event('sessao:expirada'))
      }
    }

    return Promise.reject(erro)
  },
)

export default api
