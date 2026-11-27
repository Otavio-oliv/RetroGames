export function formatarPreco(valor) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(valor)
}

export function formatarDataHora(valor) {
  if (!valor) return 'Data não disponível'

  const data = new Date(valor)

  if (Number.isNaN(data.getTime())) {
    return 'Data inválida'
  }

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(data)
}
