using System;

class Program
{
    static void Main()
    {
        string password = "admin123";
        string hash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Senha: {password}");
        Console.WriteLine($"Hash BCrypt: {hash}");
        Console.WriteLine();
        Console.WriteLine("SQL para inserir usuário admin:");
        Console.WriteLine($"INSERT INTO User (Name, Email, Password, IsActive, CreatedAt) VALUES ('Administrador', 'admin@abrechozeira.com', '{hash}', 1, NOW());");
    }
}
