export function Carregando({ mensagem = 'Carregando...' }) {
  return (
    <div className="text-center py-5" role="status">
      <div className="spinner-border text-primary" aria-hidden="true" />
      <p className="mt-3 mb-0">{mensagem}</p>
    </div>
  )
}

export function MensagemErro({ mensagem, aoTentarNovamente }) {
  return (
    <div className="alert alert-danger" role="alert">
      <p className="mb-3">{mensagem}</p>

      {aoTentarNovamente && (
        <button
          type="button"
          className="btn btn-outline-danger"
          onClick={aoTentarNovamente}
        >
          Tentar novamente
        </button>
      )}
    </div>
  )
}

export function EstadoVazio({ mensagem = 'Nenhum resultado encontrado.' }) {
  return (
    <div className="alert alert-info text-center" role="status">
      {mensagem}
    </div>
  )
}
