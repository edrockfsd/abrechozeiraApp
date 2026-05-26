using MySqlConnector;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Server=mysql.abrechozeira.com.br;Port=3306;Database=abrechozeira01;User=abrechozeira01;Password=cqjl5gi8;AllowUserVariables=true;";
        
        Console.WriteLine("=== Inserindo dados iniciais ===\n");
        
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            Console.WriteLine("✓ Conexão estabelecida!\n");

            // Lista de comandos SQL
            var commands = new[] {
                // Níveis de Acesso
                "INSERT IGNORE INTO NivelAcesso (Id, Descricao) VALUES (1, 'Administrador')",
                "INSERT IGNORE INTO NivelAcesso (Id, Descricao) VALUES (2, 'Gerente')",
                "INSERT IGNORE INTO NivelAcesso (Id, Descricao) VALUES (3, 'Vendedor')",
                "INSERT IGNORE INTO NivelAcesso (Id, Descricao) VALUES (4, 'Visualizador')",
                
                // Origens
                "INSERT IGNORE INTO Origem (Id, Descricao) VALUES (1, 'Loja Física')",
                "INSERT IGNORE INTO Origem (Id, Descricao) VALUES (2, 'Instagram')",
                "INSERT IGNORE INTO Origem (Id, Descricao) VALUES (3, 'Live')",
                "INSERT IGNORE INTO Origem (Id, Descricao) VALUES (4, 'WhatsApp')",
                "INSERT IGNORE INTO Origem (Id, Descricao) VALUES (5, 'Site')",
                
                // PessoaCategoria
                "INSERT IGNORE INTO PessoaCategoria (Id, Descricao) VALUES (1, 'Cliente')",
                "INSERT IGNORE INTO PessoaCategoria (Id, Descricao) VALUES (2, 'Fornecedor')",
                "INSERT IGNORE INTO PessoaCategoria (Id, Descricao) VALUES (3, 'Funcionário')",
                "INSERT IGNORE INTO PessoaCategoria (Id, Descricao) VALUES (4, 'Outro')",
                
                // PessoaTipo
                "INSERT IGNORE INTO PessoaTipo (Id, Descricao) VALUES (1, 'Física')",
                "INSERT IGNORE INTO PessoaTipo (Id, Descricao) VALUES (2, 'Jurídica')",
                
                // PessoaGenero
                "INSERT IGNORE INTO PessoaGenero (Id, Sigla, Descricao) VALUES (1, 'M', 'Masculino')",
                "INSERT IGNORE INTO PessoaGenero (Id, Sigla, Descricao) VALUES (2, 'F', 'Feminino')",
                "INSERT IGNORE INTO PessoaGenero (Id, Sigla, Descricao) VALUES (3, 'U', 'Unissex')",
                "INSERT IGNORE INTO PessoaGenero (Id, Sigla, Descricao) VALUES (4, 'O', 'Outro')",
                
                // PessoaStatus
                "INSERT IGNORE INTO PessoaStatus (Id, Descricao) VALUES (1, 'Ativo')",
                "INSERT IGNORE INTO PessoaStatus (Id, Descricao) VALUES (2, 'Inativo')",
                "INSERT IGNORE INTO PessoaStatus (Id, Descricao) VALUES (3, 'Bloqueado')",
                
                // PessoaPerfil
                "INSERT IGNORE INTO PessoaPerfil (Id, Descricao) VALUES (1, 'Normal')",
                "INSERT IGNORE INTO PessoaPerfil (Id, Descricao) VALUES (2, 'VIP')",
                "INSERT IGNORE INTO PessoaPerfil (Id, Descricao) VALUES (3, 'Atacado')",
                
                // TipoEndereco
                "INSERT IGNORE INTO TipoEndereco (Id, Descricao) VALUES (1, 'Residencial')",
                "INSERT IGNORE INTO TipoEndereco (Id, Descricao) VALUES (2, 'Comercial')",
                "INSERT IGNORE INTO TipoEndereco (Id, Descricao) VALUES (3, 'Entrega')",
                "INSERT IGNORE INTO TipoEndereco (Id, Descricao) VALUES (4, 'Cobrança')",
                
                // ProdutoGrupo
                "INSERT IGNORE INTO ProdutoGrupo (Id, Descricao) VALUES (1, 'Roupas')",
                "INSERT IGNORE INTO ProdutoGrupo (Id, Descricao) VALUES (2, 'Calçados')",
                "INSERT IGNORE INTO ProdutoGrupo (Id, Descricao) VALUES (3, 'Acessórios')",
                "INSERT IGNORE INTO ProdutoGrupo (Id, Descricao) VALUES (4, 'Bolsas')",
                "INSERT IGNORE INTO ProdutoGrupo (Id, Descricao) VALUES (5, 'Infantil')",
                "INSERT IGNORE INTO ProdutoGrupo (Id, Descricao) VALUES (6, 'Outros')",
                
                // ProdutoStatus
                "INSERT IGNORE INTO ProdutoStatus (Id, Descricao) VALUES (1, 'Ativo')",
                "INSERT IGNORE INTO ProdutoStatus (Id, Descricao) VALUES (2, 'Inativo')",
                "INSERT IGNORE INTO ProdutoStatus (Id, Descricao) VALUES (3, 'Vendido')",
                "INSERT IGNORE INTO ProdutoStatus (Id, Descricao) VALUES (4, 'Reservado')",
                
                // ProdutoPerfil
                "INSERT IGNORE INTO ProdutoPerfil (Id, Descricao) VALUES (1, 'Adulto')",
                "INSERT IGNORE INTO ProdutoPerfil (Id, Descricao) VALUES (2, 'Infantil')",
                "INSERT IGNORE INTO ProdutoPerfil (Id, Descricao) VALUES (3, 'Plus Size')",
                
                // ProdutoMarca
                "INSERT IGNORE INTO ProdutoMarca (Id, Descricao) VALUES (1, 'Sem Marca')",
                "INSERT IGNORE INTO ProdutoMarca (Id, Descricao) VALUES (2, 'Zara')",
                "INSERT IGNORE INTO ProdutoMarca (Id, Descricao) VALUES (3, 'Renner')",
                "INSERT IGNORE INTO ProdutoMarca (Id, Descricao) VALUES (4, 'C&A')",
                "INSERT IGNORE INTO ProdutoMarca (Id, Descricao) VALUES (5, 'Hering')",
                
                // Pessoa Admin
                "INSERT IGNORE INTO Pessoa (Id, Nome, Email, Telefone, PessoaCategoriaId, PessoaTipoId, StatusId, PessoaGeneroId, DataInclusao) VALUES (1, 'Administrador', 'admin@abrechozeira.com', '', 3, 1, 1, 4, NOW())",
                
                // Usuario Admin
                "INSERT IGNORE INTO Usuario (Id, Login, Senha, NivelAcessoID, PessoaID) VALUES (1, 'admin', 'admin123', 1, 1)",
                
                // FormaPagamento
                "INSERT IGNORE INTO FormaPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (1, 'Dinheiro', NOW(), 1)",
                "INSERT IGNORE INTO FormaPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (2, 'PIX', NOW(), 1)",
                "INSERT IGNORE INTO FormaPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (3, 'Cartão de Crédito', NOW(), 1)",
                "INSERT IGNORE INTO FormaPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (4, 'Cartão de Débito', NOW(), 1)",
                "INSERT IGNORE INTO FormaPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (5, 'Transferência', NOW(), 1)",
                
                // CondicaoPagamento
                "INSERT IGNORE INTO CondicaoPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (1, 'À Vista', NOW(), 1)",
                "INSERT IGNORE INTO CondicaoPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (2, '2x sem juros', NOW(), 1)",
                "INSERT IGNORE INTO CondicaoPagamento (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (3, '3x sem juros', NOW(), 1)",
                
                // PedidoStatus
                "INSERT IGNORE INTO PedidoStatus (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (1, 'Aberto', NOW(), 1)",
                "INSERT IGNORE INTO PedidoStatus (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (2, 'Em Preparo', NOW(), 1)",
                "INSERT IGNORE INTO PedidoStatus (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (3, 'Enviado', NOW(), 1)",
                "INSERT IGNORE INTO PedidoStatus (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (4, 'Entregue', NOW(), 1)",
                "INSERT IGNORE INTO PedidoStatus (Id, Descricao, DataAlteracao, UsuarioModificacaoId) VALUES (5, 'Cancelado', NOW(), 1)"
            };
            
            int count = 0;
            foreach (var sql in commands)
            {
                try
                {
                    using var cmd = new MySqlCommand(sql, connection);
                    await cmd.ExecuteNonQueryAsync();
                    count++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Aviso: {ex.Message.Substring(0, Math.Min(80, ex.Message.Length))}");
                }
            }
            Console.WriteLine($"✓ {count} comandos executados\n");
            
            // Verificar
            Console.WriteLine("Verificando dados inseridos:");
            var tables = new[] { "NivelAcesso", "Origem", "PessoaCategoria", "ProdutoGrupo", "FormaPagamento", "Usuario" };
            foreach (var table in tables)
            {
                using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {table}", connection);
                var c = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"  {table}: {c} registros");
            }
            
            Console.WriteLine("\n=== Banco de produção configurado! ===");
            Console.WriteLine("\nUsuário admin criado:");
            Console.WriteLine("  Login: admin");
            Console.WriteLine("  Senha: admin123");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO: {ex.Message}");
        }
    }
}
