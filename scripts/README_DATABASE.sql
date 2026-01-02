-- ============================================
-- SCRIPT DE CRIAÇÃO DO BANCO + SEED
-- A.Brechozeira - Para produção Kinghost
-- ============================================

-- INSTRUÇÕES DE USO:
-- 1. Na Kinghost, crie um novo banco MySQL pelo painel
-- 2. Anote: nome do banco, usuário e senha
-- 3. Conecte ao banco via phpMyAdmin ou cliente MySQL
-- 4. Execute primeiro: script.sql (estrutura das tabelas)
-- 5. Execute depois: seed_database.sql (dados iniciais)

-- ============================================
-- ORDEM DE EXECUÇÃO:
-- ============================================

-- PASSO 1: Criar estrutura (execute: script.sql)
-- source c:/Users/eduar/GIT_REPOS/abrechozeiraApp/script.sql;

-- PASSO 2: Inserir dados iniciais (execute: seed_database.sql)
-- source c:/Users/eduar/GIT_REPOS/abrechozeiraApp/scripts/seed_database.sql;

-- PASSO 3: (Opcional) Aplicar migrations do EF Core para tabelas VendaPdv
-- Execute na API: dotnet ef database update

-- ============================================
-- PARA RESETAR O BANCO (limpar dados de teste):
-- ============================================
-- source c:/Users/eduar/GIT_REPOS/abrechozeiraApp/scripts/reset_database.sql;
-- Depois execute novamente o seed_database.sql se necessário

SELECT 'Leia as instruções no início deste arquivo!' AS Instrucoes;
