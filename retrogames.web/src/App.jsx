import { Routes, Route } from 'react-router-dom'
import Layout from './layouts/Layout'
import NaoEncontrada from './pages/NaoEncontrada'

function Catalogo() {
  return <h1>Catálogo de Jogos</h1>
}

function DetalheJogo() {
  return <h1>Detalhes do Jogo</h1>
}

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Catalogo />} />
        <Route path="/jogos/:id" element={<DetalheJogo />} />
        <Route path="*" element={<NaoEncontrada />} />
      </Route>
    </Routes>
  )
}

export default App
