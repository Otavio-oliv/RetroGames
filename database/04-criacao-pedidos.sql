
-- ============================================================
-- RetroGames - Estrutura de pedidos
-- ============================================================

-- ------------------------------------------------------------
-- 1. Situações dos pedidos
-- ------------------------------------------------------------

CREATE TABLE IF NOT EXISTS tb_situacao_pedido (
    co_situacao_pedido INT PRIMARY KEY,
    ds_situacao_pedido VARCHAR(50) NOT NULL
);

-- ------------------------------------------------------------
-- 2. Pedidos
-- ------------------------------------------------------------

CREATE TABLE IF NOT EXISTS tb_pedido (
    co_seq_pedido INT AUTO_INCREMENT PRIMARY KEY,
    dt_pedido DATETIME NOT NULL,
    vl_total DECIMAL(10,2) NOT NULL,
    co_usuario INT NOT NULL,
    co_situacao_pedido INT NOT NULL,
    FOREIGN KEY (co_usuario)
        REFERENCES tb_usuario(co_seq_usuario),
    FOREIGN KEY (co_situacao_pedido)
        REFERENCES tb_situacao_pedido(co_situacao_pedido)
);

-- ------------------------------------------------------------
-- 3. Itens dos pedidos
-- ------------------------------------------------------------

CREATE TABLE IF NOT EXISTS tb_item_pedido (
    co_seq_item_pedido INT AUTO_INCREMENT PRIMARY KEY,
    qt_item INT NOT NULL,
    vl_unitario DECIMAL(10,2) NOT NULL,
    co_pedido INT NOT NULL,
    co_jogo INT NOT NULL,
    UNIQUE (co_pedido, co_jogo),
    FOREIGN KEY (co_pedido)
        REFERENCES tb_pedido(co_seq_pedido),
    FOREIGN KEY (co_jogo)
        REFERENCES tb_jogo(co_seq_jogo)
);

-- ------------------------------------------------------------
-- 4. Situações iniciais
-- ------------------------------------------------------------

INSERT INTO tb_situacao_pedido
    (co_situacao_pedido, ds_situacao_pedido)
VALUES
    (1, 'Pendente'),
    (2, 'Pago'),
    (3, 'Enviado'),
    (4, 'Cancelado');
