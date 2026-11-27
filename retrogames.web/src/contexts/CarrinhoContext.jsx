
import {
  createContext,
  useContext,
  useEffect,
  useState,
} from 'react'

const CHAVE_CARRINHO = 'retrogames:carrinho:v1'

const CarrinhoContext = createContext(null)

function carregarCarrinhoInicial() {
  try {
    const dados = localStorage.getItem(CHAVE_CARRINHO)

    if (!dados) return []

    const itens = JSON.parse(dados)

    if (!Array.isArray(itens)) return []

    return itens.filter((item) => (
      Number.isInteger(item.id) &&
      item.id > 0 &&
      typeof item.nome === 'string' &&
      Number.isFinite(Number(item.preco)) &&
      Number(item.preco) >= 0 &&
      Number.isInteger(item.quantidade) &&
      item.quantidade >= 1 &&
      item.quantidade <= 10
    ))
  } catch {
    return []
  }
}

export function CarrinhoProvider({ children }) {
  const [itens, setItens] = useState(carregarCarrinhoInicial)

  useEffect(() => {
    try {
      localStorage.setItem(
        CHAVE_CARRINHO,
        JSON.stringify(itens),
      )
    } catch {
      // O carrinho continua disponível durante a sessão.
    }
  }, [itens])

  function adicionarItem(jogo) {
    const id = Number(jogo?.id)
    const estoque = Number(jogo?.estoque)
    const preco = Number(jogo?.preco)

    if (!Number.isInteger(id) || id <= 0) {
      return {
        sucesso: false,
        mensagem: 'Não foi possível identificar o jogo.',
      }
    }

    if (!jogo.ativo || !Number.isInteger(estoque) || estoque <= 0) {
      return {
        sucesso: false,
        mensagem: 'Este jogo não está disponível para compra.',
      }
    }

    if (!Number.isFinite(preco) || preco < 0) {
      return {
        sucesso: false,
        mensagem: 'O preço deste jogo é inválido.',
      }
    }

    const itemExistente = itens.find((item) => item.id === id)
    const quantidadeAtual = itemExistente
      ? itemExistente.quantidade
      : 0

    const limite = Math.min(10, estoque)

    if (quantidadeAtual >= limite) {
      return {
        sucesso: false,
        mensagem: `Limite disponível: ${limite} unidade(s).`,
      }
    }

    const dadosJogo = {
      id,
      nome: jogo.nome,
      preco,
      estoque,
      ativo: Boolean(jogo.ativo),
      nomePlataforma: jogo.nomePlataforma || '',
    }

    if (itemExistente) {
      setItens((itensAtuais) =>
        itensAtuais.map((item) =>
          item.id === id
            ? {
                ...dadosJogo,
                quantidade: item.quantidade + 1,
              }
            : item,
        ),
      )
    } else {
      setItens((itensAtuais) => [
        ...itensAtuais,
        {
          ...dadosJogo,
          quantidade: 1,
        },
      ])
    }

    return {
      sucesso: true,
      mensagem: 'Jogo adicionado ao carrinho.',
    }
  }

  function alterarQuantidade(id, novaQuantidade) {
    const quantidade = Number(novaQuantidade)

    if (!Number.isInteger(quantidade)) return

    setItens((itensAtuais) =>
      itensAtuais.map((item) => {
        if (item.id !== Number(id)) return item

        const limite = Math.min(10, item.estoque)

        return {
          ...item,
          quantidade: Math.max(1, Math.min(quantidade, limite)),
        }
      }),
    )
  }

  function removerItem(id) {
    setItens((itensAtuais) =>
      itensAtuais.filter((item) => item.id !== Number(id)),
    )
  }

  function limparCarrinho() {
    setItens([])
  }

  const quantidadeTotal = itens.reduce(
    (total, item) => total + item.quantidade,
    0,
  )

  const valorTotal = itens.reduce(
    (total, item) => total + item.preco * item.quantidade,
    0,
  )

  const valor = {
    itens,
    quantidadeTotal,
    valorTotal,
    adicionarItem,
    alterarQuantidade,
    removerItem,
    limparCarrinho,
  }

  return (
    <CarrinhoContext.Provider value={valor}>
      {children}
    </CarrinhoContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useCarrinho() {
  const contexto = useContext(CarrinhoContext)

  if (!contexto) {
    throw new Error(
      'useCarrinho deve ser utilizado dentro de CarrinhoProvider.',
    )
  }

  return contexto
}
