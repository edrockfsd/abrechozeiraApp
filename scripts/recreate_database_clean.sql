-- ============================================
-- SCRIPT DE RECRIAÇÃO DO BANCO ABRECHOZEIRA
-- Versão limpa - DROP ALL + CREATE ALL
-- ============================================
-- 
-- INSTRUÇÕES:
-- 1. Execute este script no banco de PRODUÇÃO (abrechozeira01)
-- 2. Depois execute o seed_database.sql para inserir dados iniciais
-- 
-- ⚠️ ATENÇÃO: Este script APAGA TODOS OS DADOS do banco!
-- ============================================

SET FOREIGN_KEY_CHECKS = 0;

-- ==============================
-- DROP DE TODAS AS TABELAS
-- ==============================

-- Tabelas de sistema novo de auth/permissões
DROP TABLE IF EXISTS `UserRole`;
DROP TABLE IF EXISTS `RolePermission`;
DROP TABLE IF EXISTS `User`;
DROP TABLE IF EXISTS `Role`;
DROP TABLE IF EXISTS `Permission`;

-- PDV
DROP TABLE IF EXISTS `VendaPdvPagamento`;
DROP TABLE IF EXISTS `VendaPdvItem`;
DROP TABLE IF EXISTS `VendaPdv`;
DROP TABLE IF EXISTS `CaixaMovimento`;
DROP TABLE IF EXISTS `Caixa`;
DROP TABLE IF EXISTS `FormaPagamentoConfigPDV`;

-- Transacionais
DROP TABLE IF EXISTS `Arremate`;
DROP TABLE IF EXISTS `Estoque`;
DROP TABLE IF EXISTS `PedidoProduto`;
DROP TABLE IF EXISTS `Pedido`;
DROP TABLE IF EXISTS `Venda`;
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
DROP TABLE IF EXISTS `__EFMigrationsHistory`;

SET FOREIGN_KEY_CHECKS = 1;

-- ==============================
-- CRIAÇÃO DAS TABELAS
-- ==============================

-- Migrations History
CREATE TABLE `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL PRIMARY KEY,
    `ProductVersion` varchar(32) NOT NULL
) CHARACTER SET=utf8mb4;

-- Nivel Acesso
CREATE TABLE `NivelAcesso` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(200) NOT NULL
) CHARACTER SET=utf8mb4;

-- Origem
CREATE TABLE `Origem` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(200) NOT NULL
) CHARACTER SET=utf8mb4;

-- Pessoa Categoria
CREATE TABLE `PessoaCategoria` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Pessoa Tipo
CREATE TABLE `PessoaTipo` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Pessoa Genero
CREATE TABLE `PessoaGenero` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Sigla` varchar(1) NOT NULL,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Pessoa Status
CREATE TABLE `PessoaStatus` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Pessoa Perfil
CREATE TABLE `PessoaPerfil` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Tipo Endereco
CREATE TABLE `TipoEndereco` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Produto Grupo
CREATE TABLE `ProdutoGrupo` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(100) NOT NULL
) CHARACTER SET=utf8mb4;

-- Produto Marca
CREATE TABLE `ProdutoMarca` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(100) NOT NULL
) CHARACTER SET=utf8mb4;

-- Produto Perfil
CREATE TABLE `ProdutoPerfil` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Produto Status
CREATE TABLE `ProdutoStatus` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(50) NOT NULL
) CHARACTER SET=utf8mb4;

-- Pessoa
CREATE TABLE `Pessoa` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Nome` varchar(50) NULL,
    `DataNascimento` datetime(6) NULL,
    `Email` varchar(100) NULL,
    `Telefone` varchar(13) NULL,
    `PessoaCategoriaId` int NOT NULL,
    `PessoaTipoId` int NOT NULL,
    `NickName` varchar(50) NULL,
    `DataInclusao` datetime(6) NULL,
    `StatusId` int NOT NULL DEFAULT 0,
    `PessoaGeneroId` int NOT NULL DEFAULT 0,
    `CPF` varchar(50) NULL,
    `RG` longtext NULL,
    `Observacoes` longtext NULL,
    CONSTRAINT `FK_Pessoa_PessoaCategoria` FOREIGN KEY (`PessoaCategoriaId`) REFERENCES `PessoaCategoria` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Pessoa_PessoaTipo` FOREIGN KEY (`PessoaTipoId`) REFERENCES `PessoaTipo` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Pessoa_PessoaStatus` FOREIGN KEY (`StatusId`) REFERENCES `PessoaStatus` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Pessoa_PessoaGenero` FOREIGN KEY (`PessoaGeneroId`) REFERENCES `PessoaGenero` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Pessoa_PessoaCategoriaId` ON `Pessoa` (`PessoaCategoriaId`);
CREATE INDEX `IX_Pessoa_PessoaTipoId` ON `Pessoa` (`PessoaTipoId`);
CREATE INDEX `IX_Pessoa_StatusId` ON `Pessoa` (`StatusId`);
CREATE INDEX `IX_Pessoa_PessoaGeneroId` ON `Pessoa` (`PessoaGeneroId`);

-- Usuario (tabela legada)
CREATE TABLE `Usuario` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Login` varchar(50) NULL,
    `Senha` varchar(100) NULL,
    `NivelAcessoID` int NOT NULL,
    `PessoaID` int NOT NULL,
    CONSTRAINT `FK_Usuario_NivelAcesso` FOREIGN KEY (`NivelAcessoID`) REFERENCES `NivelAcesso` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Usuario_Pessoa` FOREIGN KEY (`PessoaID`) REFERENCES `Pessoa` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Usuario_NivelAcessoID` ON `Usuario` (`NivelAcessoID`);
CREATE INDEX `IX_Usuario_PessoaID` ON `Usuario` (`PessoaID`);

-- CondicaoPagamento
CREATE TABLE `CondicaoPagamento` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` longtext NOT NULL,
    `DataAlteracao` datetime(6) NULL,
    `UsuarioModificacaoId` int NOT NULL,
    CONSTRAINT `FK_CondicaoPagamento_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_CondicaoPagamento_UsuarioModificacaoId` ON `CondicaoPagamento` (`UsuarioModificacaoId`);

-- FormaPagamento
CREATE TABLE `FormaPagamento` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` longtext NOT NULL,
    `DataAlteracao` datetime(6) NULL,
    `UsuarioModificacaoId` int NOT NULL,
    CONSTRAINT `FK_FormaPagamento_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_FormaPagamento_UsuarioModificacaoId` ON `FormaPagamento` (`UsuarioModificacaoId`);

-- PedidoStatus
CREATE TABLE `PedidoStatus` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` longtext NOT NULL,
    `DataAlteracao` datetime(6) NULL,
    `UsuarioModificacaoId` int NOT NULL,
    CONSTRAINT `FK_PedidoStatus_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_PedidoStatus_UsuarioModificacaoId` ON `PedidoStatus` (`UsuarioModificacaoId`);

-- Endereco
CREATE TABLE `Endereco` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `PessoaID` int NOT NULL,
    `CEP` varchar(8) NULL,
    `Logradouro` varchar(100) NOT NULL,
    `Unidade` varchar(8) NOT NULL,
    `Complemento` varchar(50) NULL,
    `Bairro` varchar(50) NOT NULL,
    `Localidade` varchar(50) NOT NULL,
    `CodigoLocalidadeIBGE` int NOT NULL,
    `Estado` varchar(30) NOT NULL,
    `DataAlteracao` datetime(6) NULL,
    `UsuarioModificacaoId` int NOT NULL,
    `TipoEnderecoId` int NOT NULL,
    `Observacoes` longtext NULL,
    CONSTRAINT `FK_Endereco_Pessoa` FOREIGN KEY (`PessoaID`) REFERENCES `Pessoa` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Endereco_TipoEndereco` FOREIGN KEY (`TipoEnderecoId`) REFERENCES `TipoEndereco` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Endereco_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Endereco_PessoaID` ON `Endereco` (`PessoaID`);
CREATE INDEX `IX_Endereco_TipoEnderecoId` ON `Endereco` (`TipoEnderecoId`);
CREATE INDEX `IX_Endereco_UsuarioModificacaoId` ON `Endereco` (`UsuarioModificacaoId`);

-- Live
CREATE TABLE `Live` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Titulo` varchar(50) NOT NULL,
    `Observacoes` longtext NULL,
    `DataLive` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
    `DataAlteracao` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
    `UsuarioModificacaoId` int NOT NULL DEFAULT 0,
    CONSTRAINT `FK_Live_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Live_UsuarioModificacaoId` ON `Live` (`UsuarioModificacaoId`);

-- LiveSession
CREATE TABLE `LiveSession` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `LiveVideoId` bigint NOT NULL,
    `Status` varchar(50) NOT NULL,
    `StartedAt` datetime(6) NOT NULL,
    `EndedAt` datetime(6) NULL
) CHARACTER SET=utf8mb4;

-- ComentarioLive
CREATE TABLE `ComentarioLive` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Username` varchar(200) NOT NULL,
    `CommentText` longtext NOT NULL,
    `CommentTimestamp` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LiveSessionID` bigint NULL
) CHARACTER SET=utf8mb4;

-- Produto
CREATE TABLE `Produto` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Descricao` varchar(100) NOT NULL,
    `Tamanho` varchar(50) NULL,
    `PrecoCusto` decimal(65,2) NULL,
    `PrecoVenda` decimal(18,3) NULL,
    `Origem` varchar(50) NULL,
    `GrupoID` int NOT NULL DEFAULT 0,
    `DataCompra` datetime(6) NULL,
    `DataAlteracao` datetime(6) NULL,
    `UsuarioModificacaoId` int NOT NULL DEFAULT 0,
    `StatusId` int NOT NULL DEFAULT 0,
    `MarcaId` int NULL,
    `GeneroID` int NOT NULL,
    `PerfilID` int NOT NULL,
    `Condicao` varchar(1) NULL,
    CONSTRAINT `FK_Produto_PessoaGenero` FOREIGN KEY (`GeneroID`) REFERENCES `PessoaGenero` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Produto_ProdutoGrupo` FOREIGN KEY (`GrupoID`) REFERENCES `ProdutoGrupo` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Produto_ProdutoMarca` FOREIGN KEY (`MarcaId`) REFERENCES `ProdutoMarca` (`Id`),
    CONSTRAINT `FK_Produto_ProdutoPerfil` FOREIGN KEY (`PerfilID`) REFERENCES `ProdutoPerfil` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Produto_ProdutoStatus` FOREIGN KEY (`StatusId`) REFERENCES `ProdutoStatus` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Produto_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Produto_GeneroID` ON `Produto` (`GeneroID`);
CREATE INDEX `IX_Produto_GrupoID` ON `Produto` (`GrupoID`);
CREATE INDEX `IX_Produto_MarcaId` ON `Produto` (`MarcaId`);
CREATE INDEX `IX_Produto_PerfilID` ON `Produto` (`PerfilID`);
CREATE INDEX `IX_Produto_StatusId` ON `Produto` (`StatusId`);
CREATE INDEX `IX_Produto_UsuarioModificacaoId` ON `Produto` (`UsuarioModificacaoId`);

-- Estoque
CREATE TABLE `Estoque` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Quantidade` int NOT NULL,
    `Localizacao` varchar(100) NULL,
    `DataAlteracao` datetime(6) NULL,
    `UsuarioModificacaoId` int NOT NULL DEFAULT 0,
    `ProdutoId` int NOT NULL DEFAULT 0,
    `CodigoEstoque` int NULL,
    CONSTRAINT `FK_Estoque_Produto` FOREIGN KEY (`ProdutoId`) REFERENCES `Produto` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Estoque_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Estoque_ProdutoId` ON `Estoque` (`ProdutoId`);
CREATE INDEX `IX_Estoque_UsuarioModificacaoId` ON `Estoque` (`UsuarioModificacaoId`);

-- Arremate
CREATE TABLE `Arremate` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `LiveId` int NOT NULL DEFAULT 0,
    `ProdutoId` int NOT NULL,
    `Arrematante` varchar(50) NOT NULL,
    `ValorArremate` decimal(65,30) NULL,
    `Observacoes` longtext NULL,
    `DataArremate` datetime(6) NOT NULL,
    `DataAlteracao` datetime(6) NOT NULL,
    `UsuarioModificacaoId` int NOT NULL,
    `CodigoLive` int NULL,
    CONSTRAINT `FK_Arremate_Live` FOREIGN KEY (`LiveId`) REFERENCES `Live` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Arremate_Produto` FOREIGN KEY (`ProdutoId`) REFERENCES `Produto` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Arremate_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Arremate_LiveId` ON `Arremate` (`LiveId`);
CREATE INDEX `IX_Arremate_ProdutoId` ON `Arremate` (`ProdutoId`);
CREATE INDEX `IX_Arremate_UsuarioModificacaoId` ON `Arremate` (`UsuarioModificacaoId`);

-- Venda
CREATE TABLE `Venda` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Quantidade` int NOT NULL,
    `ValorVenda` decimal(65,30) NOT NULL,
    `Desconto` decimal(65,30) NULL,
    `ClienteId` int NOT NULL,
    `ProdutoId` int NOT NULL,
    `CodigoLive` int NULL,
    `OrigemID` int NULL,
    `OrdemVendaLive` int NULL,
    `LiveId` int NULL,
    `DataAlteracao` datetime(6) NULL,
    `DataPagamento` datetime(6) NULL,
    `DataVenda` datetime(6) NULL,
    `UsuarioModificacaoId` int NOT NULL DEFAULT 0,
    CONSTRAINT `FK_Venda_Live` FOREIGN KEY (`LiveId`) REFERENCES `Live` (`Id`),
    CONSTRAINT `FK_Venda_Origem` FOREIGN KEY (`OrigemID`) REFERENCES `Origem` (`Id`),
    CONSTRAINT `FK_Venda_Pessoa` FOREIGN KEY (`ClienteId`) REFERENCES `Pessoa` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Venda_Produto` FOREIGN KEY (`ProdutoId`) REFERENCES `Produto` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Venda_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Venda_ClienteId` ON `Venda` (`ClienteId`);
CREATE INDEX `IX_Venda_LiveId` ON `Venda` (`LiveId`);
CREATE INDEX `IX_Venda_OrigemID` ON `Venda` (`OrigemID`);
CREATE INDEX `IX_Venda_ProdutoId` ON `Venda` (`ProdutoId`);
CREATE INDEX `IX_Venda_UsuarioModificacaoId` ON `Venda` (`UsuarioModificacaoId`);

-- Pedido
CREATE TABLE `Pedido` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `PedidoCodigo` int NOT NULL,
    `DataLancamento` datetime(6) NOT NULL,
    `ClienteID` int NOT NULL,
    `DescontoPorcentagem` decimal(18,3) NULL,
    `ValorFrete` decimal(18,3) NULL,
    `PedidoStatusID` int NOT NULL,
    `ValorTotal` decimal(18,3) NULL,
    `CondicaoPagamentoID` int NULL,
    `FormaPagamentoID` int NULL,
    `EnderecoEntregaID` int NULL,
    `Observacoes` longtext NULL,
    `DataAlteracao` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
    `UsuarioModificacaoId` int NOT NULL,
    CONSTRAINT `IX_Pedido_PedidoCodigo` UNIQUE (`PedidoCodigo`),
    CONSTRAINT `FK_Pedido_CondicaoPagamento` FOREIGN KEY (`CondicaoPagamentoID`) REFERENCES `CondicaoPagamento` (`Id`),
    CONSTRAINT `FK_Pedido_Endereco` FOREIGN KEY (`EnderecoEntregaID`) REFERENCES `Endereco` (`Id`),
    CONSTRAINT `FK_Pedido_FormaPagamento` FOREIGN KEY (`FormaPagamentoID`) REFERENCES `FormaPagamento` (`Id`),
    CONSTRAINT `FK_Pedido_PedidoStatus` FOREIGN KEY (`PedidoStatusID`) REFERENCES `PedidoStatus` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Pedido_Pessoa` FOREIGN KEY (`ClienteID`) REFERENCES `Pessoa` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Pedido_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Pedido_ClienteID` ON `Pedido` (`ClienteID`);
CREATE INDEX `IX_Pedido_CondicaoPagamentoID` ON `Pedido` (`CondicaoPagamentoID`);
CREATE INDEX `IX_Pedido_EnderecoEntregaID` ON `Pedido` (`EnderecoEntregaID`);
CREATE INDEX `IX_Pedido_FormaPagamentoID` ON `Pedido` (`FormaPagamentoID`);
CREATE INDEX `IX_Pedido_PedidoStatusID` ON `Pedido` (`PedidoStatusID`);
CREATE INDEX `IX_Pedido_UsuarioModificacaoId` ON `Pedido` (`UsuarioModificacaoId`);

-- PedidoProduto
CREATE TABLE `PedidoProduto` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `PedidoId` int NOT NULL,
    `ProdutoId` int NOT NULL,
    `Quantidade` int NOT NULL,
    `DescontoValor` decimal(18,3) NULL,
    `ValorFinalProduto` decimal(18,3) NULL,
    `DataAlteracao` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
    `UsuarioModificacaoId` int NOT NULL,
    CONSTRAINT `FK_PedidoProduto_Pedido` FOREIGN KEY (`PedidoId`) REFERENCES `Pedido` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PedidoProduto_Produto` FOREIGN KEY (`ProdutoId`) REFERENCES `Produto` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PedidoProduto_Usuario` FOREIGN KEY (`UsuarioModificacaoId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_PedidoProduto_PedidoId` ON `PedidoProduto` (`PedidoId`);
CREATE INDEX `IX_PedidoProduto_ProdutoId` ON `PedidoProduto` (`ProdutoId`);
CREATE INDEX `IX_PedidoProduto_UsuarioModificacaoId` ON `PedidoProduto` (`UsuarioModificacaoId`);

-- ==============================
-- SISTEMA DE AUTENTICAÇÃO NOVO
-- ==============================

-- Permission
CREATE TABLE `Permission` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Name` varchar(100) NOT NULL,
    `Description` varchar(500) NULL,
    `Resource` varchar(100) NOT NULL,
    `Action` varchar(100) NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime(6) NULL
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Permission_Name` ON `Permission` (`Name`);
CREATE INDEX `IX_Permission_Resource` ON `Permission` (`Resource`);
CREATE INDEX `IX_Permission_Action` ON `Permission` (`Action`);
CREATE INDEX `IX_Permission_IsActive` ON `Permission` (`IsActive`);

-- Role
CREATE TABLE `Role` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Name` varchar(50) NOT NULL,
    `Description` varchar(200) NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime(6) NULL
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Role_Name` ON `Role` (`Name`);
CREATE INDEX `IX_Role_IsActive` ON `Role` (`IsActive`);

-- User (novo sistema)
CREATE TABLE `User` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Name` varchar(100) NOT NULL,
    `Email` varchar(255) NOT NULL,
    `Password` varchar(255) NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime(6) NULL,
    `PessoaId` int NOT NULL,
    CONSTRAINT `FK_User_Pessoa` FOREIGN KEY (`PessoaId`) REFERENCES `Pessoa` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_User_Email` ON `User` (`Email`);
CREATE UNIQUE INDEX `IX_User_PessoaId` ON `User` (`PessoaId`);
CREATE INDEX `IX_User_IsActive` ON `User` (`IsActive`);
CREATE INDEX `IX_User_CreatedAt` ON `User` (`CreatedAt`);

-- RolePermission
CREATE TABLE `RolePermission` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `RoleId` int NOT NULL,
    `PermissionId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `FK_RolePermission_Role` FOREIGN KEY (`RoleId`) REFERENCES `Role` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RolePermission_Permission` FOREIGN KEY (`PermissionId`) REFERENCES `Permission` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_RolePermission_RoleId_PermissionId` ON `RolePermission` (`RoleId`, `PermissionId`);

-- UserRole
CREATE TABLE `UserRole` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `UserId` int NOT NULL,
    `RoleId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `FK_UserRole_User` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserRole_Role` FOREIGN KEY (`RoleId`) REFERENCES `Role` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_UserRole_UserId_RoleId` ON `UserRole` (`UserId`, `RoleId`);

-- ==============================
-- PDV TABLES
-- ==============================

-- Caixa
CREATE TABLE `Caixa` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `DataAbertura` datetime(6) NOT NULL,
    `DataFechamento` datetime(6) NULL,
    `SaldoInicial` decimal(18,2) NOT NULL,
    `SaldoFinal` decimal(18,2) NULL,
    `Status` varchar(20) NOT NULL,
    `UsuarioId` int NOT NULL,
    CONSTRAINT `FK_Caixa_Usuario` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuario` (`Id`)
) CHARACTER SET=utf8mb4;

-- CaixaMovimento
CREATE TABLE `CaixaMovimento` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `CaixaId` int NOT NULL,
    `TipoMovimento` varchar(20) NOT NULL,
    `Valor` decimal(18,2) NOT NULL,
    `Descricao` varchar(200) NULL,
    `DataMovimento` datetime(6) NOT NULL,
    CONSTRAINT `FK_CaixaMovimento_Caixa` FOREIGN KEY (`CaixaId`) REFERENCES `Caixa` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- FormaPagamentoConfigPDV
CREATE TABLE `FormaPagamentoConfigPDV` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `FormaPagamentoId` int NOT NULL,
    `AceitaTroco` tinyint(1) NOT NULL DEFAULT 0,
    `PermiteParcelamento` tinyint(1) NOT NULL DEFAULT 0,
    `MaxParcelas` int NULL,
    `Ativo` tinyint(1) NOT NULL DEFAULT 1,
    CONSTRAINT `FK_FormaPagamentoConfigPDV_FormaPagamento` FOREIGN KEY (`FormaPagamentoId`) REFERENCES `FormaPagamento` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- VendaPdv
CREATE TABLE `VendaPdv` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `CaixaId` int NOT NULL,
    `ClienteId` int NULL,
    `DataVenda` datetime(6) NOT NULL,
    `SubTotal` decimal(18,2) NOT NULL,
    `Desconto` decimal(18,2) NOT NULL DEFAULT 0,
    `Total` decimal(18,2) NOT NULL,
    `Status` varchar(20) NOT NULL,
    `UsuarioId` int NOT NULL,
    `Observacoes` longtext NULL,
    CONSTRAINT `FK_VendaPdv_Caixa` FOREIGN KEY (`CaixaId`) REFERENCES `Caixa` (`Id`),
    CONSTRAINT `FK_VendaPdv_Pessoa` FOREIGN KEY (`ClienteId`) REFERENCES `Pessoa` (`Id`),
    CONSTRAINT `FK_VendaPdv_Usuario` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuario` (`Id`)
) CHARACTER SET=utf8mb4;

-- VendaPdvItem
CREATE TABLE `VendaPdvItem` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `VendaPdvId` int NOT NULL,
    `ProdutoId` int NOT NULL,
    `Quantidade` int NOT NULL,
    `PrecoUnitario` decimal(18,2) NOT NULL,
    `Desconto` decimal(18,2) NOT NULL DEFAULT 0,
    `Total` decimal(18,2) NOT NULL,
    CONSTRAINT `FK_VendaPdvItem_VendaPdv` FOREIGN KEY (`VendaPdvId`) REFERENCES `VendaPdv` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_VendaPdvItem_Produto` FOREIGN KEY (`ProdutoId`) REFERENCES `Produto` (`Id`)
) CHARACTER SET=utf8mb4;

-- VendaPdvPagamento
CREATE TABLE `VendaPdvPagamento` (
    `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `VendaPdvId` int NOT NULL,
    `FormaPagamentoId` int NOT NULL,
    `Valor` decimal(18,2) NOT NULL,
    `Parcelas` int NULL,
    `Troco` decimal(18,2) NULL,
    CONSTRAINT `FK_VendaPdvPagamento_VendaPdv` FOREIGN KEY (`VendaPdvId`) REFERENCES `VendaPdv` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_VendaPdvPagamento_FormaPagamento` FOREIGN KEY (`FormaPagamentoId`) REFERENCES `FormaPagamento` (`Id`)
) CHARACTER SET=utf8mb4;

-- ==============================
-- VERIFICAÇÃO FINAL
-- ==============================
SELECT 'Estrutura do banco criada com sucesso!' AS Status;

SELECT 
    table_name AS Tabela,
    table_rows AS Registros
FROM information_schema.tables 
WHERE table_schema = DATABASE()
ORDER BY table_name;
