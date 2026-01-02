-- ============================================
-- SCRIPT PARA LIMPAR BANCO DE PRODUCAO
-- Dropa todas as tabelas na ordem correta (FKs)
-- ============================================

SET FOREIGN_KEY_CHECKS = 0;

-- PDV
DROP TABLE IF EXISTS `VendaPdvPagamento`;
DROP TABLE IF EXISTS `VendaPdvItem`;
DROP TABLE IF EXISTS `VendaPdv`;
DROP TABLE IF EXISTS `CaixaMovimento`;
DROP TABLE IF EXISTS `Caixa`;
DROP TABLE IF EXISTS `FormaPagamentoConfigPDV`;

-- Auth novo
DROP TABLE IF EXISTS `UserRole`;
DROP TABLE IF EXISTS `RolePermission`;
DROP TABLE IF EXISTS `User`;
DROP TABLE IF EXISTS `Role`;
DROP TABLE IF EXISTS `Permission`;

-- Transacionais
DROP TABLE IF EXISTS `PedidoProduto`;
DROP TABLE IF EXISTS `Pedido`;
DROP TABLE IF EXISTS `Arremate`;
DROP TABLE IF EXISTS `Venda`;
DROP TABLE IF EXISTS `Estoque`;
DROP TABLE IF EXISTS `ComentarioLive`;
DROP TABLE IF EXISTS `LiveSession`;
DROP TABLE IF EXISTS `Live`;
DROP TABLE IF EXISTS `Endereco`;
DROP TABLE IF EXISTS `Produto`;

-- Cadastros
DROP TABLE IF EXISTS `Usuario`;
DROP TABLE IF EXISTS `Pessoa`;

-- Lookups
DROP TABLE IF EXISTS `TipoEndereco`;
DROP TABLE IF EXISTS `ProdutoMarca`;
DROP TABLE IF EXISTS `ProdutoGrupo`;
DROP TABLE IF EXISTS `ProdutoPerfil`;
DROP TABLE IF EXISTS `ProdutoStatus`;
DROP TABLE IF EXISTS `PedidoStatus`;
DROP TABLE IF EXISTS `FormaPagamento`;
DROP TABLE IF EXISTS `CondicaoPagamento`;
DROP TABLE IF EXISTS `Origem`;
DROP TABLE IF EXISTS `PessoaPerfil`;
DROP TABLE IF EXISTS `PessoaStatus`;
DROP TABLE IF EXISTS `PessoaGenero`;
DROP TABLE IF EXISTS `PessoaTipo`;
DROP TABLE IF EXISTS `PessoaCategoria`;
DROP TABLE IF EXISTS `NivelAcesso`;

-- EF Migrations
DROP TABLE IF EXISTS `__EFMigrationsHistory`;

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Banco limpo com sucesso!' AS Status;
