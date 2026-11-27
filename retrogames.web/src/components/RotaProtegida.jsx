import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { useAutenticacao } from '../contexts/AutenticacaoContext'

function RotaProtegida({
  somenteAdministrador = false,
  somenteCliente = false,
}) {
  const location = useLocation()

  const {
    usuario,
    estaCarregando,
    estaAutenticado,
    ehAdministrador,
  } = useAutenticacao()

  if (estaCarregando) {
    return (
      <div className="text-center py-5">
        Verificando sessão...
      </div>
    )
  }

  if (!estaAutenticado) {
    const voltarPara =
      location.pathname + location.search

    return (
      <Navigate
        to={`/login?voltarPara=${encodeURIComponent(voltarPara)}`}
        replace
      />
    )
  }

  if (somenteAdministrador && !ehAdministrador) {
    return <Navigate to="/" replace />
  }

  if (
    somenteCliente &&
    usuario?.perfil !== 'Cliente'
  ) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}

export default RotaProtegida
