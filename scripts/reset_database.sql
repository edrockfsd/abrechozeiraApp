-- ============================================
-- SCRIPT DE RESET DO BANCO DE DADOS
-- A.Brechozeira - Limpar dados de teste
-- ============================================

-- IMPORTANTE: Este script apaga TODOS os dados transacionais
-- Mantenha apenas os dados de lookup/configuração

SET FOREIGN_KEY_CHECKS = 0;

-- ==============================
-- 1. LIMPAR TABELAS TRANSACIONAIS
-- ==============================

-- Vendas PDV (se existirem as tabelas)
-- Comente as linhas abaixo se as tabelas não existirem
-- TRUNCATE TABLE VendaPdvItem;
-- TRUNCATE TABLE VendaPdvPagamento;
-- TRUNCATE TABLE VendaPdv;

-- Executar apenas se as tabelas VendaPdv existirem:
SET @tbl_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'VendaPdvItem');
SET @sql = IF(@tbl_exists > 0, 'TRUNCATE TABLE VendaPdvItem', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @tbl_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'VendaPdvPagamento');
SET @sql = IF(@tbl_exists > 0, 'TRUNCATE TABLE VendaPdvPagamento', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @tbl_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'VendaPdv');
SET @sql = IF(@tbl_exists > 0, 'TRUNCATE TABLE VendaPdv', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Vendas (legado)
TRUNCATE TABLE Venda;

-- Arremates
TRUNCATE TABLE Arremate;

-- Pedidos
TRUNCATE TABLE PedidoProduto;
TRUNCATE TABLE Pedido;

-- Lives
TRUNCATE TABLE ComentarioLive;
TRUNCATE TABLE LiveSession;
TRUNCATE TABLE Live;

-- Estoque
TRUNCATE TABLE Estoque;

-- Produtos
TRUNCATE TABLE Produto;

-- Enderecos (manter estrutura, limpar dados)
TRUNCATE TABLE Endereco;

-- Pessoas (exceto usuários do sistema)
-- NÃO usar TRUNCATE aqui por causa das FKs com Usuario
DELETE FROM Pessoa WHERE Id NOT IN (SELECT PessoaID FROM Usuario);

SET FOREIGN_KEY_CHECKS = 1;

-- ==============================
-- VERIFICAÇÃO
-- ==============================
SELECT 'Dados limpos com sucesso!' AS Status;

SELECT 
    (SELECT COUNT(*) FROM VendaPdv) AS VendasPdv,
    (SELECT COUNT(*) FROM Venda) AS Vendas,
    (SELECT COUNT(*) FROM Pedido) AS Pedidos,
    (SELECT COUNT(*) FROM Produto) AS Produtos,
    (SELECT COUNT(*) FROM Estoque) AS Estoque,
    (SELECT COUNT(*) FROM Pessoa) AS Pessoas,
    (SELECT COUNT(*) FROM Usuario) AS Usuarios;
