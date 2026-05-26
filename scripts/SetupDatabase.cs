using MySqlConnector;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Server=mysql.abrechozeira.com.br;Port=3306;Database=abrechozeira01;User=abrechozeira01;Password=cqjl5gi8;AllowUserVariables=true;";
        
        Console.WriteLine("=== Configuração do Banco de Produção ===\n");
        
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            Console.WriteLine("✓ Conexão estabelecida com sucesso!\n");
            
            // 1. Executar script de estrutura
            Console.WriteLine("1. Criando estrutura das tabelas...");
            var scriptPath = @"c:\Users\eduar\GIT_REPOS\abrechozeiraApp\scripts\script.sql";
            if (File.Exists(scriptPath))
            {
                var script = await File.ReadAllTextAsync(scriptPath);
                // Dividir por comandos
                var commands = script.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                foreach (var cmd in commands)
                {
                    var trimmed = cmd.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;
                    try
                    {
                        using var command = new MySqlCommand(trimmed + ";", connection);
                        command.CommandTimeout = 120;
                        await command.ExecuteNonQueryAsync();
                        count++;
                    }
                    catch (Exception ex)
                    {
                        // Ignora erros de tabela já existente
                        if (!ex.Message.Contains("already exists"))
                        {
                            Console.WriteLine($"  Aviso: {ex.Message.Substring(0, Math.Min(100, ex.Message.Length))}");
                        }
                    }
                }
                Console.WriteLine($"  ✓ {count} comandos executados\n");
            }
            else
            {
                Console.WriteLine($"  ✗ Arquivo não encontrado: {scriptPath}\n");
            }
            
            // 2. Executar seed
            Console.WriteLine("2. Inserindo dados iniciais...");
            var seedPath = @"c:\Users\eduar\GIT_REPOS\abrechozeiraApp\scripts\seed_database.sql";
            if (File.Exists(seedPath))
            {
                var seed = await File.ReadAllTextAsync(seedPath);
                var commands = seed.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                foreach (var cmd in commands)
                {
                    var trimmed = cmd.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("--") || trimmed.StartsWith("SELECT")) continue;
                    try
                    {
                        using var command = new MySqlCommand(trimmed + ";", connection);
                        command.CommandTimeout = 60;
                        await command.ExecuteNonQueryAsync();
                        count++;
                    }
                    catch (Exception ex)
                    {
                        // Ignora erros de duplicata
                        if (!ex.Message.Contains("Duplicate"))
                        {
                            Console.WriteLine($"  Aviso: {ex.Message.Substring(0, Math.Min(100, ex.Message.Length))}");
                        }
                    }
                }
                Console.WriteLine($"  ✓ {count} registros inseridos\n");
            }
            
            // 3. Verificar resultados
            Console.WriteLine("3. Verificando dados inseridos...");
            var tables = new[] { "NivelAcesso", "Origem", "PessoaCategoria", "ProdutoGrupo", "FormaPagamento", "Usuario" };
            foreach (var table in tables)
            {
                using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {table}", connection);
                var count = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"  {table}: {count} registros");
            }
            
            Console.WriteLine("\n=== Configuração concluída com sucesso! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO: {ex.Message}");
        }
    }
}
