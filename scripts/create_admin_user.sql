-- ============================================
-- SCRIPT DE CRIAÇÃO DO USUÁRIO ADMIN
-- A.Brechozeira - Usuário com senha BCrypt
-- ============================================
-- 
-- Execute este script no banco de dados após as migrations
-- Login: admin@abrechozeira.com
-- Senha: admin123
--
-- ============================================

-- Inserir usuário admin com senha BCrypt
INSERT INTO User (Name, Email, Password, IsActive, CreatedAt) 
VALUES (
    'Administrador', 
    'admin@abrechozeira.com', 
    '$2a$11$2svI1eSVMi/OLMrf1zqHOOYCq.oyp70.uLirCVNIgVrnEwML33RE2', 
    1, 
    NOW()
);

-- Criar Role de Admin se não existir
INSERT INTO Role (Name, Description, IsActive, CreatedAt)
VALUES ('ADMIN', 'Administrador do Sistema', 1, NOW())
ON DUPLICATE KEY UPDATE Name = Name;

-- Associar usuário à role de Admin
INSERT INTO UserRole (UserId, RoleId)
SELECT 
    (SELECT Id FROM User WHERE Email = 'admin@abrechozeira.com'),
    (SELECT Id FROM Role WHERE Name = 'ADMIN');

-- Verificar inserção
SELECT 'Usuário admin criado com sucesso!' AS Status;
SELECT Id, Name, Email, IsActive FROM User WHERE Email = 'admin@abrechozeira.com';
