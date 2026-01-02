using AbiaApp.API.Services;
using Dapper;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1. Adiciona suporte a Controllers
builder.Services.AddControllers();

// 2. Adiciona suporte ao Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. (NOVO) Adiciona política de CORS permissiva para desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});



var app = builder.Build();

// --- CARREGAMENTO DO CACHE ---
using (var scope = app.Services.CreateScope())
{
    // ⚠️ CONFIRA SUA SENHA DO BANCO AQUI
    

    try
    {
        using (var conn = new MySqlConnection(connString))
        {
            conn.Open();
            // Dentro do bloco "using (var conn = new MySqlConnection(connString))" no Program.cs:

            Console.WriteLine("--> [SISTEMA] Conectando ao MySQL para carregar cache...");

            // CORREÇÃO: Usando 'Descricao' em vez de 'Nome'
            CacheSistema.Grupos = conn.Query<ItemDominio>("SELECT Id, Descricao as Nome FROM ProdutoGrupo").ToList();
            CacheSistema.Marcas = conn.Query<ItemDominio>("SELECT Id, Descricao as Nome FROM ProdutoMarca").ToList();
            CacheSistema.Generos = conn.Query<ItemDominio>("SELECT Id, Descricao as Nome FROM PessoaGenero").ToList();
            CacheSistema.Perfis = conn.Query<ItemDominio>("SELECT Id, Descricao as Nome FROM ProdutoPerfil").ToList();

            Console.WriteLine($"--> [SUCESSO] Cache carregado! {CacheSistema.Marcas.Count} marcas prontas.");

            Console.WriteLine($"--> [SUCESSO] Cache carregado! {CacheSistema.Marcas.Count} marcas prontas.");
        }
    }
    catch (Exception ex)
    {
        // Se der erro aqui, aparecerá no terminal preto
        Console.WriteLine($"--> [ERRO NO BANCO] {ex.Message}");
    }
}
// -----------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. (NOVO) Aplica a política de CORS
app.UseCors("PermitirTudo");

// 5. (ALTERADO) Comentei essa linha para evitar o erro "Failed to Fetch" no HTTP local
// app.UseHttpsRedirection(); 

app.UseAuthorization();

// --- ADICIONE ESTAS DUAS LINHAS AQUI ---
app.UseDefaultFiles(); // Procura por index.html
app.UseStaticFiles();  // Permite entregar arquivos da pasta wwwroot
// ---------------------------------------

app.MapControllers();

app.Run();