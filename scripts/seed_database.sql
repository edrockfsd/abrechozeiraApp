-- ============================================
-- SCRIPT DE DADOS INICIAIS (SEED)
-- A.Brechozeira - Dados para novo banco
-- ============================================
--
-- Execute DEPOIS do recreate_database_clean.sql
--
-- Usuário admin criado:
-- Login: admin@abrechozeira.com
-- Senha: admin123
-- ============================================

-- ==============================
-- 1. NÍVEIS DE ACESSO
-- ==============================
INSERT INTO NivelAcesso (Id, Descricao) VALUES
(1, 'Administrador'),
(2, 'Gerente'),
(3, 'Vendedor'),
(4, 'Visualizador');

-- ==============================
-- 2. ORIGENS DE VENDA
-- ==============================
INSERT INTO Origem (Id, Descricao) VALUES
(1, 'Loja Física'),
(2, 'Instagram'),
(3, 'Live'),
(4, 'WhatsApp'),
(5, 'Site');

-- ==============================
-- 3. CATEGORIAS DE PESSOA
-- ==============================
INSERT INTO PessoaCategoria (Id, Descricao) VALUES
(1, 'Cliente'),
(2, 'Fornecedor'),
(3, 'Funcionário'),
(4, 'Outro');

-- ==============================
-- 4. TIPO DE PESSOA
-- ==============================
INSERT INTO PessoaTipo (Id, Descricao) VALUES
(1, 'Física'),
(2, 'Jurídica');

-- ==============================
-- 5. GÊNERO
-- ==============================
INSERT INTO PessoaGenero (Id, Sigla, Descricao) VALUES
(1, 'M', 'Masculino'),
(2, 'F', 'Feminino'),
(3, 'U', 'Unissex'),
(4, 'O', 'Outro');

-- ==============================
-- 6. STATUS DE PESSOA
-- ==============================
INSERT INTO PessoaStatus (Id, Descricao) VALUES
(1, 'Ativo'),
(2, 'Inativo'),
(3, 'Bloqueado');

-- ==============================
-- 7. PERFIL DE PESSOA
-- ==============================
INSERT INTO PessoaPerfil (Id, Descricao) VALUES
(1, 'Normal'),
(2, 'VIP'),
(3, 'Atacado');

-- ==============================
-- 8. TIPO DE ENDEREÇO
-- ==============================
INSERT INTO TipoEndereco (Id, Descricao) VALUES
(1, 'Residencial'),
(2, 'Comercial'),
(3, 'Entrega'),
(4, 'Cobrança');

-- ==============================
-- 9. GRUPOS DE PRODUTO
-- ==============================
INSERT INTO ProdutoGrupo (Id, Descricao) VALUES
(1, 'Roupas'),
(2, 'Calçados'),
(3, 'Acessórios'),
(4, 'Bolsas'),
(5, 'Infantil'),
(6, 'Outros');

-- ==============================
-- 10. STATUS DE PRODUTO
-- ==============================
INSERT INTO ProdutoStatus (Id, Descricao) VALUES
(1, 'Ativo'),
(2, 'Inativo'),
(3, 'Vendido'),
(4, 'Reservado');

-- ==============================
-- 11. PERFIL DE PRODUTO
-- ==============================
INSERT INTO ProdutoPerfil (Id, Descricao) VALUES
(1, 'Adulto'),
(2, 'Infantil'),
(3, 'Plus Size');

-- ==============================
-- 12. MARCAS COMUNS
-- ==============================
INSERT INTO ProdutoMarca (Id, Descricao) VALUES
(1, 'Sem Marca'),
(2, 'Zara'),
(3, 'Renner'),
(4, 'C&A'),
(5, 'Hering'),
(6, 'Farm'),
(7, 'Animale'),
(8, 'Nike'),
(9, 'Adidas'),
(10, 'Outras');

-- ==============================
-- 13. PESSOA ADMIN (para FK do Usuario legado)
-- ==============================
INSERT INTO Pessoa (Id, Nome, Email, Telefone, PessoaCategoriaId, PessoaTipoId, StatusId, PessoaGeneroId, DataInclusao) VALUES
(1, 'Administrador', 'admin@abrechozeira.com', '', 3, 1, 1, 4, NOW());

-- ==============================
-- 14. USUÁRIO ADMIN (tabela legada)
-- ==============================
INSERT INTO Usuario (Id, Login, Senha, NivelAcessoID, PessoaID) VALUES
(1, 'admin', 'admin123', 1, 1);

-- ==============================
-- 15. FORMAS DE PAGAMENTO
-- ==============================
INSERT INTO FormaPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES
(1, 'Dinheiro', NOW(), 1),
(2, 'PIX', NOW(), 1),
(3, 'Cartão de Crédito', NOW(), 1),
(4, 'Cartão de Débito', NOW(), 1),
(5, 'Transferência', NOW(), 1);

-- ==============================
-- 16. CONDIÇÕES DE PAGAMENTO
-- ==============================
INSERT INTO CondicaoPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES
(1, 'À Vista', NOW(), 1),
(2, '2x sem juros', NOW(), 1),
(3, '3x sem juros', NOW(), 1),
(4, 'Entrada + 1x', NOW(), 1);

-- ==============================
-- 17. STATUS DE PEDIDO
-- ==============================
INSERT INTO PedidoStatus (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES
(1, 'Aberto', NOW(), 1),
(2, 'Em Preparo', NOW(), 1),
(3, 'Enviado', NOW(), 1),
(4, 'Entregue', NOW(), 1),
(5, 'Cancelado', NOW(), 1);

-- ==============================
-- 18. PERMISSÕES DO SISTEMA
-- ==============================
INSERT INTO Permission (Id, Name, Description, Resource, Action, IsActive) VALUES
(1, 'produtos_create', 'Criar produtos', 'produtos', 'CREATE', 1),
(2, 'produtos_read', 'Visualizar produtos', 'produtos', 'READ', 1),
(3, 'produtos_update', 'Atualizar produtos', 'produtos', 'UPDATE', 1),
(4, 'produtos_delete', 'Deletar produtos', 'produtos', 'DELETE', 1),
(5, 'estoque_manage', 'Gerenciar estoque', 'estoque', 'CREATE,UPDATE', 1),
(6, 'estoque_read', 'Visualizar estoque', 'estoque', 'READ', 1),
(7, 'pedidos_create', 'Criar pedidos', 'pedidos', 'CREATE', 1),
(8, 'pedidos_read', 'Visualizar pedidos', 'pedidos', 'READ', 1),
(9, 'clientes_create', 'Cadastrar clientes', 'clientes', 'CREATE', 1),
(10, 'clientes_read', 'Visualizar clientes', 'clientes', 'READ', 1),
(11, 'relatorios_read', 'Visualizar relatórios', 'relatorios', 'READ', 1),
(99, 'full_access', 'Acesso completo a todos os recursos', '*', '*', 1);

-- ==============================
-- 19. ROLES DO SISTEMA
-- ==============================
INSERT INTO Role (Id, Name, Description, IsActive) VALUES
(1, 'ADMIN', 'Acesso total ao sistema', 1),
(2, 'MANAGER', 'Gerente com acesso a produtos, estoque e pedidos', 1),
(3, 'SELLER', 'Vendedor com acesso a vendas e visualização', 1),
(4, 'VIEWER', 'Apenas visualização de dados', 1);

-- ==============================
-- 20. ROLE PERMISSIONS
-- ==============================
INSERT INTO RolePermission (Id, RoleId, PermissionId) VALUES
-- ADMIN tem full_access
(1, 1, 99),
-- MANAGER
(2, 2, 1), -- produtos_create
(3, 2, 2), -- produtos_read
(4, 2, 3), -- produtos_update
(5, 2, 5), -- estoque_manage
(6, 2, 7), -- pedidos_create
-- SELLER
(7, 3, 2), -- produtos_read
(8, 3, 6), -- estoque_read
(9, 3, 7), -- pedidos_create
(10, 3, 9), -- clientes_create
-- VIEWER
(11, 4, 2), -- produtos_read
(12, 4, 6), -- estoque_read
(13, 4, 11); -- relatorios_read

-- ==============================
-- 21. USUARIO ADMIN (NOVO SISTEMA - com BCrypt)
-- ==============================
-- Senha: admin123
-- Hash BCrypt: $2a$11$2svI1eSVMi/OLMrf1zqHOOYCq.oyp70.uLirCVNIgVrnEwML33RE2
-- PessoaId = 1 (Administrador criado no passo 13)
INSERT INTO User (Id, Name, Email, Password, IsActive, PessoaId) VALUES
(1, 'Administrador', 'admin@abrechozeira.com', '$2a$11$2svI1eSVMi/OLMrf1zqHOOYCq.oyp70.uLirCVNIgVrnEwML33RE2', 1, 1);

-- ==============================
-- 22. USER ROLE (ADMIN)
-- ==============================
INSERT INTO UserRole (UserId, RoleId) VALUES
(1, 1);

-- ==============================
-- VERIFICAÇÃO FINAL
-- ==============================
SELECT 'Dados iniciais inseridos com sucesso!' AS Status;

SELECT 
    (SELECT COUNT(*) FROM NivelAcesso) AS NiveisAcesso,
    (SELECT COUNT(*) FROM Origem) AS Origens,
    (SELECT COUNT(*) FROM PessoaCategoria) AS Categorias,
    (SELECT COUNT(*) FROM ProdutoGrupo) AS Grupos,
    (SELECT COUNT(*) FROM FormaPagamento) AS FormasPagamento,
    (SELECT COUNT(*) FROM Permission) AS Permissoes,
    (SELECT COUNT(*) FROM Role) AS Roles,
    (SELECT COUNT(*) FROM User) AS Users,
    (SELECT COUNT(*) FROM Usuario) AS UsuariosLegado;
