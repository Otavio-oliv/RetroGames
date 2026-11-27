import { useParams } from 'react-router-dom'

function FormularioJogo() {
  const { id } = useParams()

  const estaEditando = Boolean(id)

  return (
    <div>
      <h1>
        {estaEditando ? 'Editar jogo' : 'Novo jogo'}
      </h1>

      <p className="text-secondary">
        {estaEditando
          ? `Edição do jogo ${id}.`
          : 'Cadastro de um novo jogo.'}
      </p>
    </div>
  )
}

export default FormularioJogo
