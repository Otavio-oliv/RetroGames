import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'

import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'

import App from './App.jsx'
import { AutenticacaoProvider } from './contexts/AutenticacaoContext'
import { CarrinhoProvider } from './contexts/CarrinhoContext'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <AutenticacaoProvider>
        <CarrinhoProvider>
          <App />
        </CarrinhoProvider>
      </AutenticacaoProvider>
    </BrowserRouter>
  </StrictMode>,
)
