
-- ============================================================
-- RetroGames - Seed inicial
-- Etapa 4 do roteiro
-- ============================================================

USE db2025108034;

-- ------------------------------------------------------------
-- 1. Condições dos jogos
-- ------------------------------------------------------------

INSERT INTO tb_condicao_jogo
    (co_condicao_jogo, ds_condicao_jogo)
VALUES
    (1, 'Novo'),
    (2, 'Usado'),
    (3, 'Restaurado');

-- ------------------------------------------------------------
-- 2. Perfis
-- ------------------------------------------------------------

INSERT INTO tb_perfil
    (co_perfil, ds_perfil)
VALUES
    (1, 'Administrador'),
    (2, 'Cliente');

-- ------------------------------------------------------------
-- 3. Plataformas
-- ------------------------------------------------------------

INSERT INTO tb_plataforma
    (co_seq_plataforma, no_plataforma, no_fabricante, nu_ano_lancamento)
VALUES
    (1, 'NES', 'Nintendo', 1983),
    (2, 'Super Nintendo', 'Nintendo', 1990),
    (3, 'Mega Drive', 'Sega', 1988),
    (4, 'Atari 2600', 'Atari', 1977),
    (5, 'PlayStation', 'Sony', 1994),
    (6, 'Game Boy', 'Nintendo', 1989);

-- ------------------------------------------------------------
-- 4. Gêneros
-- ------------------------------------------------------------

INSERT INTO tb_genero
    (co_seq_genero, no_genero)
VALUES
    (1, 'Plataforma'),
    (2, 'Aventura'),
    (3, 'RPG'),
    (4, 'Ação'),
    (5, 'Corrida'),
    (6, 'Luta'),
    (7, 'Puzzle'),
    (8, 'Esporte');

-- ------------------------------------------------------------
-- 5. Jogos
-- ------------------------------------------------------------

INSERT INTO tb_jogo
    (
        co_seq_jogo,
        no_jogo,
        ds_jogo,
        nu_ano_lancamento,
        vl_preco,
        qt_estoque,
        st_ativo,
        co_plataforma,
        co_condicao_jogo
    )
VALUES
    (1, 'Super Mario Bros.', 'Clássico de plataforma da Nintendo.', 1985, 120.00, 5, TRUE, 1, 2),
    (2, 'The Legend of Zelda', 'Aventura em Hyrule.', 1986, 150.00, 3, TRUE, 1, 2),
    (3, 'Metroid', 'Aventura e exploração espacial.', 1986, 140.00, 0, TRUE, 1, 3),
    (4, 'Mega Man 2', 'Ação e plataforma.', 1988, 130.00, 4, TRUE, 1, 2),
    (5, 'Super Mario World', 'Aventura de Mario no SNES.', 1990, 180.00, 6, TRUE, 2, 2),
    (6, 'The Legend of Zelda: A Link to the Past', 'Aventura clássica no SNES.', 1991, 220.00, 2, TRUE, 2, 2),
    (7, 'Donkey Kong Country', 'Plataforma com gráficos pré-renderizados.', 1994, 190.00, 3, TRUE, 2, 2),
    (8, 'Street Fighter II', 'Jogo clássico de luta.', 1991, 160.00, 0, TRUE, 2, 3),
    (9, 'Sonic the Hedgehog', 'Aventura de alta velocidade.', 1991, 130.00, 5, TRUE, 3, 2),
    (10, 'Streets of Rage 2', 'Ação beat em up.', 1992, 170.00, 2, TRUE, 3, 2),
    (11, 'Golden Axe', 'Ação e fantasia.', 1989, 110.00, 4, TRUE, 3, 3),
    (12, 'Phantasy Star IV', 'RPG espacial.', 1993, 250.00, 1, TRUE, 3, 2),
    (13, 'Pac-Man', 'Puzzle e labirinto.', 1980, 90.00, 3, TRUE, 4, 2),
    (14, 'Space Invaders', 'Jogo clássico de tiro espacial.', 1978, 85.00, 0, TRUE, 4, 3),
    (15, 'Pitfall!', 'Aventura na selva.', 1982, 80.00, 2, TRUE, 4, 3),
    (16, 'Crash Bandicoot', 'Plataforma em 3D.', 1996, 200.00, 4, TRUE, 5, 2),
    (17, 'Tekken 3', 'Jogo de luta.', 1997, 180.00, 3, TRUE, 5, 2),
    (18, 'Gran Turismo', 'Simulador de corrida.', 1997, 160.00, 2, TRUE, 5, 2),
    (19, 'Tetris', 'Puzzle clássico.', 1989, 100.00, 6, TRUE, 6, 2),
    (20, 'Pokémon Red', 'RPG de captura de criaturas.', 1996, 230.00, 0, FALSE, 6, 2);

-- ------------------------------------------------------------
-- 6. Relacionamento entre jogos e gêneros
-- ------------------------------------------------------------

INSERT INTO tb_jogo_genero (co_jogo, co_genero)
VALUES
    (1, 1),
    (2, 2),
    (2, 3),
    (3, 2),
    (3, 4),
    (4, 1),
    (4, 4),
    (5, 1),
    (5, 2),
    (6, 2),
    (6, 3),
    (7, 1),
    (7, 2),
    (8, 6),
    (9, 1),
    (9, 2),
    (10, 4),
    (10, 6),
    (11, 4),
    (11, 2),
    (12, 3),
    (13, 7),
    (14, 4),
    (15, 2),
    (16, 1),
    (16, 2),
    (17, 6),
    (18, 5),
    (19, 7),
    (20, 3);

-- ============================================================
-- Fim do seed inicial
-- ============================================================
