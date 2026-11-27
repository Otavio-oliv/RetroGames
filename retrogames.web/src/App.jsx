
import { Routes, Route } from 'react-router-dom'

import Layout from './layouts/Layout'
import RotaProtegida from './components/RotaProtegida'

import Catalogo from './pages/Catalogo'
import DetalheJogo from './pages/DetalheJogo'
import Login from './pages/Login'
import NaoEncontrada from './pages/NaoEncontrada'

import Jogos from './pages/Administracao/Jogos'
import FormularioJogo from './pages/Administracao/FormularioJogo'
import Plataforma from './pages/Auxiliares/Plataforma'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Catalogo />} />
        <Route path="/jogos/:id" element={<DetalheJogo />} />
        <Route path="/login" element={<Login />} />

        <Route
          path="/admin"
          element={<RotaProtegida somenteAdministrador />}
        >
          <Route path="jogos" element={<Jogos />} />

          <Route
            path="jogos/novo"
            element={<FormularioJogo />}
          />

          <Route
            path="jogos/:id/editar"
            element={<FormularioJogo />}
          />

          <Route
            path="plataformas"
            element={<Plataforma />}
          />
        </Route>

        <Route path="*" element={<NaoEncontrada />} />
      </Route>
    </Routes>
  )
}

export default App
