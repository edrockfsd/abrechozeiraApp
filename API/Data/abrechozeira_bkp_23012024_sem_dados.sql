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

-- Copiando estrutura para tabela abrechozeira.Venda
CREATE TABLE IF NOT EXISTS `Venda` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Quantidade` int NOT NULL,
  `PrecoVenda` decimal(65,30) NOT NULL,
  `Desconto` decimal(65,30) DEFAULT NULL,
  `ClienteId` int NOT NULL,
  `ProdutoId` int NOT NULL,
  `CodigoLive` int DEFAULT NULL,
  `OrigemID` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_Venda_ClienteId` (`ClienteId`),
  KEY `IX_Venda_ProdutoId` (`ProdutoId`),
  KEY `IX_Venda_OrigemID` (`OrigemID`),
  CONSTRAINT `FK_Venda_Pessoa_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Pessoa` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Venda_Produto_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produto` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
