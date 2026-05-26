using MySqlConnector;

var cs1 = "Server=mysql.abrechozeira.com.br;Port=3306;Database=abrechozeira;Uid=abrechozeira;Pwd=drkm6hj9;";
var cs2 = "Server=mysql.abrechozeira.com.br;Port=3306;Database=abrechozeira01;Uid=abrechozeira01;Pwd=Cqjl5Gi8;";

async Task<List<string>> GetTables(string cs, string db)
{
    using var conn = new MySqlConnection(cs);
    await conn.OpenAsync();
    using var cmd = new MySqlCommand($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='{db}' ORDER BY TABLE_NAME", conn);
    using var reader = await cmd.ExecuteReaderAsync();
    var tables = new List<string>();
    while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
    return tables;
}

async Task<int> GetRowCount(string cs, string table)
{
    using var conn = new MySqlConnection(cs);
    await conn.OpenAsync();
    using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{table}`", conn);
    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
}

var t1 = await GetTables(cs1, "abrechozeira");
var t2 = await GetTables(cs2, "abrechozeira01");

Console.WriteLine($"=== abrechozeira: {t1.Count} tabelas ===");
Console.WriteLine($"=== abrechozeira01: {t2.Count} tabelas ===");
Console.WriteLine();

var only1 = t1.Except(t2).ToList();
var only2 = t2.Except(t1).ToList();
if (only1.Any()) { Console.WriteLine("TABELAS so em abrechozeira:"); foreach(var t in only1) Console.WriteLine($"  - {t}"); }
if (only2.Any()) { Console.WriteLine("TABELAS so em abrechozeira01:"); foreach(var t in only2) Console.WriteLine($"  - {t}"); }

var common = t1.Intersect(t2).ToList();
Console.WriteLine($"\nTabelas em comum: {common.Count}");
Console.WriteLine("\n{0,-35} {1,10} {2,10}", "TABELA", "DB1", "DB2");
Console.WriteLine(new string('-', 57));
foreach (var table in common)
{
    if (table == "__EFMigrationsHistory") continue;
    try
    {
        var c1 = await GetRowCount(cs1, table);
        var c2 = await GetRowCount(cs2, table);
        var marker = c1 != c2 ? " <-- DIFERENTE" : "";
        Console.WriteLine($"{table,-35} {c1,10} {c2,10}{marker}");
    }
    catch { Console.WriteLine($"{table,-35} {"ERRO",10} {"ERRO",10}"); }
}
