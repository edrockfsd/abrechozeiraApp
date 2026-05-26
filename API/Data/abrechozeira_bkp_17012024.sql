-- --------------------------------------------------------
-- Servidor:                     db4free.net
-- Versão do servidor:           8.3.0 - MySQL Community Server - GPL
-- OS do Servidor:               Linux
-- HeidiSQL Versão:              12.6.0.6765
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Copiando estrutura do banco de dados para abrechozeira
CREATE DATABASE IF NOT EXISTS `abrechozeira` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `abrechozeira`;

-- Copiando estrutura para tabela abrechozeira.NivelAcesso
CREATE TABLE IF NOT EXISTS `NivelAcesso` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(20) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.Pessoa
CREATE TABLE IF NOT EXISTS `Pessoa` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Tipo` int NOT NULL,
  `Categoria` int NOT NULL,
  `Nome` varchar(50) DEFAULT NULL,
  `DataNascimento` datetime DEFAULT NULL,
  `Email` varchar(50) DEFAULT NULL,
  `Telefone` varchar(50) DEFAULT NULL,
  `Sexo` char(1) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_Pessoa_PessoaTipo` (`Tipo`) USING BTREE,
  KEY `FK_Pessoa_PessoaCategoria` (`Categoria`),
  CONSTRAINT `FK_Pessoa_PessoaCategoria` FOREIGN KEY (`Categoria`) REFERENCES `PessoaCategoria` (`ID`),
  CONSTRAINT `FK_Pessoa_PessoaTipo` FOREIGN KEY (`Tipo`) REFERENCES `PessoaTipo` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.PessoaCategoria
CREATE TABLE IF NOT EXISTS `PessoaCategoria` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(50) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.PessoaTipo
CREATE TABLE IF NOT EXISTS `PessoaTipo` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.Produto
CREATE TABLE IF NOT EXISTS `Produto` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(50) NOT NULL,
  `Tamanho` varchar(50) DEFAULT NULL,
  `Grupo` varchar(50) DEFAULT NULL,
  `PrecoCusto` decimal(7,2) DEFAULT NULL,
  `PrecoVenda` decimal(7,2) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.Statuses
CREATE TABLE IF NOT EXISTS `Statuses` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `StatusOpcao` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.Tarefas
CREATE TABLE IF NOT EXISTS `Tarefas` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Comentario` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TarefaTipoId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Tarefas_TarefaTipoId` (`TarefaTipoId`),
  CONSTRAINT `FK_Tarefas_TarefaTipos_TarefaTipoId` FOREIGN KEY (`TarefaTipoId`) REFERENCES `TarefaTipos` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.TarefaTipos
CREATE TABLE IF NOT EXISTS `TarefaTipos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TarefaNome` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.Usuario
CREATE TABLE IF NOT EXISTS `Usuario` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `PessoaID` int NOT NULL,
  `Login` varchar(50) DEFAULT NULL,
  `Senha` varchar(50) DEFAULT NULL,
  `NivelAcesso` int DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK__Pessoa` (`PessoaID`),
  KEY `FK_Usuario_NivelAcesso` (`NivelAcesso`),
  CONSTRAINT `FK__Pessoa` FOREIGN KEY (`PessoaID`) REFERENCES `Pessoa` (`ID`),
  CONSTRAINT `FK_Usuario_NivelAcesso` FOREIGN KEY (`NivelAcesso`) REFERENCES `NivelAcesso` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.Venda
CREATE TABLE IF NOT EXISTS `Venda` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Cliente` int NOT NULL,
  `Produto` int NOT NULL,
  `Quantidade` int NOT NULL,
  `PrecoVenda` decimal(7,2) NOT NULL,
  `Desconto` decimal(7,2) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_Venda_Pessoa` (`Cliente`),
  KEY `FK_Venda_Produto` (`Produto`),
  CONSTRAINT `FK_Venda_Pessoa` FOREIGN KEY (`Cliente`) REFERENCES `Pessoa` (`ID`),
  CONSTRAINT `FK_Venda_Produto` FOREIGN KEY (`Produto`) REFERENCES `Produto` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela abrechozeira.__EFMigrationsHistory
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
