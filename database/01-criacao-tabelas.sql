
-- RetroGames: criação das tabelas

CREATE TABLE IF NOT EXISTS tb_condicao_jogo (
    co_condicao_jogo INT PRIMARY KEY,
    ds_condicao_jogo VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS tb_perfil (
    co_perfil INT PRIMARY KEY,
    ds_perfil VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS tb_plataforma (
    co_seq_plataforma INT AUTO_INCREMENT PRIMARY KEY,
    no_plataforma VARCHAR(100) NOT NULL,
    no_fabricante VARCHAR(100) NOT NULL,
    nu_ano_lancamento SMALLINT NOT NULL,
    UNIQUE (no_plataforma)
);

CREATE TABLE IF NOT EXISTS tb_genero (
    co_seq_genero INT AUTO_INCREMENT PRIMARY KEY,
    no_genero VARCHAR(50) NOT NULL,
    UNIQUE (no_genero)
);

CREATE TABLE IF NOT EXISTS tb_jogo (
    co_seq_jogo INT AUTO_INCREMENT PRIMARY KEY,
    no_jogo VARCHAR(150) NOT NULL,
    ds_jogo TEXT NULL,
    nu_ano_lancamento SMALLINT NOT NULL,
    vl_preco DECIMAL(10,2) NOT NULL,
    qt_estoque INT NOT NULL,
    st_ativo BOOLEAN NOT NULL DEFAULT TRUE,
    dt_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    co_plataforma INT NOT NULL,
    co_condicao_jogo INT NOT NULL,
    FOREIGN KEY (co_plataforma)
        REFERENCES tb_plataforma(co_seq_plataforma),
    FOREIGN KEY (co_condicao_jogo)
        REFERENCES tb_condicao_jogo(co_condicao_jogo)
);

CREATE TABLE IF NOT EXISTS tb_jogo_genero (
    co_jogo INT NOT NULL,
    co_genero INT NOT NULL,
    PRIMARY KEY (co_jogo, co_genero),
    FOREIGN KEY (co_jogo)
        REFERENCES tb_jogo(co_seq_jogo),
    FOREIGN KEY (co_genero)
        REFERENCES tb_genero(co_seq_genero)
);

CREATE TABLE IF NOT EXISTS tb_usuario (
    co_seq_usuario INT AUTO_INCREMENT PRIMARY KEY,
    no_usuario VARCHAR(100) NOT NULL,
    ds_email VARCHAR(150) NOT NULL,
    ds_senha_hash VARCHAR(255) NOT NULL,
    st_ativo BOOLEAN NOT NULL DEFAULT TRUE,
    dt_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    co_perfil INT NOT NULL,
    UNIQUE (ds_email),
    FOREIGN KEY (co_perfil)
        REFERENCES tb_perfil(co_perfil)
);
