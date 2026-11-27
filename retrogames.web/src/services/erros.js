
export function obterMensagemErro(erro) {
  if (!erro.response) {
    return 'Não foi possível conectar ao servidor.'
  }

  return (
    erro.response.data?.errorMessage ||
    'Ocorreu um erro inesperado.'
  )
}

export function obterErrosDosCampos(erro) {
  if (erro.response?.data?.responseCode !== 3) {
    return {}
  }

  const dados = erro.response.data?.data

  if (!dados || typeof dados !== 'object' || Array.isArray(dados)) {
    return {}
  }

  return Object.fromEntries(
    Object.entries(dados).map(([campo, mensagens]) => [
      campo,
      Array.isArray(mensagens)
        ? mensagens[0]
        : String(mensagens),
    ]),
  )
}
