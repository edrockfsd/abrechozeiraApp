using AbiaApp.API.Models;
using AbiaApp.API.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AbiaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgenteController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // O construtor recebe a configuração do sistema (onde está o appsettings.json)
        public AgenteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private const int ID_MARCA_GENERICA = 10;
        private const int ID_GRUPO_GENERICO = 18;

        // --- MÉTODOS AUXILIARES PARA LER SEGREDOS ---
        // Isso impede que suas senhas fiquem escritas no código
        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        private string GetGeminiKey()
        {
            return _configuration["Gemini:ApiKey"];
        }
        // ---------------------------------------------

        [HttpGet("origens")]
        public IActionResult ListarOrigens()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            var origens = conn.Query<string>("SELECT DISTINCT Origem FROM Produto WHERE Origem IS NOT NULL AND Origem <> '' ORDER BY Origem");
            return Ok(origens);
        }

        [HttpPost("cadastrar")]
        public async Task<IActionResult> CadastrarProduto([FromBody] PedidoCadastro pedido)
        {
            if (string.IsNullOrEmpty(pedido.TextoUsuario))
                return BadRequest("Texto vazio.");

            try
            {
                // 1. Processa a IA
                var produtoEstruturado = await ProcessarInteligencia(pedido.TextoUsuario);

                // 2. Lógica de Origem Fixa ou Automática
                if (!string.IsNullOrEmpty(pedido.OrigemFixa))
                {
                    produtoEstruturado.Origem = pedido.OrigemFixa;
                }
                else if (string.IsNullOrEmpty(produtoEstruturado.Origem))
                {
                    produtoEstruturado.Origem = "IA_Voice";
                }

                // 3. Salva no Banco
                var idNovo = SalvarNoBanco(produtoEstruturado);

                return Ok(new
                {
                    mensagem = "Produto cadastrado com sucesso",
                    id = idNovo,
                    dados = produtoEstruturado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpDelete("excluir/{id}")]
        public IActionResult ExcluirPorId(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                // 1. Remove do Estoque
                var sqlEstoque = "DELETE FROM Estoque WHERE ProdutoId = @id";
                conn.Execute(sqlEstoque, new { id }, trans);

                // 2. Remove do Produto
                // Nota: Mantive 'ID' conforme seu código enviado. 
                // Se no banco for 'ProdutoID', altere aqui.
                var sqlProduto = "DELETE FROM Produto WHERE ID = @id";
                int afetados = conn.Execute(sqlProduto, new { id }, trans);

                trans.Commit();

                if (afetados > 0)
                    return Ok(new { mensagem = $"Produto {id} e estoque excluídos." });
                else
                    return NotFound(new { erro = "Produto não encontrado." });
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpDelete("excluir/ultimo")]
        public IActionResult ExcluirUltimo()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            // Busca o maior ID (o último inserido)
            var ultimoId = conn.QueryFirstOrDefault<int?>("SELECT MAX(ID) FROM Produto");

            if (ultimoId.HasValue && ultimoId.Value > 0)
            {
                return ExcluirPorId(ultimoId.Value);
            }

            return NotFound(new { erro = "Nenhum produto encontrado." });
        }

        [HttpGet("testar-modelos")]
        public async Task<string> ListarModelosDisponiveis()
        {
            using var client = new HttpClient();
            var apiKey = GetGeminiKey(); // Pega do appsettings
            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";

            try
            {
                var response = await client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }
        }

        private async Task<ProdutoIA_IDs> ProcessarInteligencia(string textoUsuario)
        {
            using var client = new HttpClient();
            var apiKey = GetGeminiKey(); // Pega do appsettings

            // Modelo Gemini 2.5 Flash
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            // --- LOGS DE DEBUG NO CONSOLE ---
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"[DEBUG] Texto: {textoUsuario}");
            Console.WriteLine($"[DEBUG] Marcas em Cache: {CacheSistema.Marcas.Count}");
            Console.WriteLine($"[DEBUG] Grupos em Cache: {CacheSistema.Grupos.Count}");
            Console.WriteLine("--------------------------------------------------");

            var jsonMarcas = JsonConvert.SerializeObject(CacheSistema.Marcas);
            var jsonGrupos = JsonConvert.SerializeObject(CacheSistema.Grupos);

            var prompt = $@"
                Atue como um especialista em cadastro de produtos de brechó.
                
                --- TABELAS DE DOMÍNIO ---
                MARCAS: {jsonMarcas}
                GRUPOS: {jsonGrupos}
                --------------------------

                ENTRADA: '{textoUsuario}'

                MISSÃO: Estruturar dados para banco SQL.
                
                REGRAS:
                1. Fuzzy Match IDs: Encontre a Marca/Grupo mais próximos na lista.
                   - Se falhar Marca: use {ID_MARCA_GENERICA}.
                   - Se falhar Grupo: use {ID_GRUPO_GENERICO}.
                
                2. Descrição OBRIGATÓRIA:
                   - Formato: [Grupo/Tipo] + [Cor] + [Marca] + [Detalhes]
                   - Ex: 'Blusa branca The North Face', NUNCA 'branca The North Face'.
                
                3. Dados:
                   - PrecoCusto: Se não falado, 0.00.
                   - Origem: Se não falado, null.
                   - Condicao: 'N' (Novo) ou 'U' (Usado).

                RETORNE JSON:
                {{
                    ""Descricao"": ""string"",
                    ""Tamanho"": ""string"",
                    ""PrecoVenda"": 0.00,
                    ""PrecoCusto"": 0.00,
                    ""Origem"": null,
                    ""MarcaId"": 0,
                    ""GrupoId"": 0,
                    ""GeneroId"": 1,
                    ""PerfilId"": 1,
                    ""Condicao"": ""U""
                }}";

            var body = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro Google API: {responseString}");

            var jsonResponse = JObject.Parse(responseString);
            var text = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            if (string.IsNullOrEmpty(text)) throw new Exception("IA retornou vazio.");

            text = text.Replace("```json", "").Replace("```", "").Trim();

            return JsonConvert.DeserializeObject<ProdutoIA_IDs>(text);
        }

        private int SalvarNoBanco(ProdutoIA_IDs dados)
        {
            using var conn = new MySqlConnection(GetConnectionString()); // Pega do appsettings
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                if (dados.MarcaId == 0) dados.MarcaId = ID_MARCA_GENERICA;
                if (dados.GrupoId == 0) dados.GrupoId = ID_GRUPO_GENERICO;
                if (dados.GeneroId == 0) dados.GeneroId = 1;
                if (dados.PerfilId == 0) dados.PerfilId = 1;
                int statusId = 1;

                var sqlProduto = @"
                    INSERT INTO Produto 
                    (
                        Descricao, Tamanho, PrecoCusto, PrecoVenda, Origem, 
                        GrupoID, DataCompra, DataAlteracao, StatusId, 
                        MarcaId, GeneroID, PerfilID, Condicao
                    )
                    VALUES 
                    (
                        @Descricao, @Tamanho, @PrecoCusto, @PrecoVenda, @Origem, 
                        @GrupoId, NOW(), NOW(), @StatusId, 
                        @MarcaId, @GeneroId, @PerfilId, @Condicao
                    );
                    SELECT LAST_INSERT_ID();";

                int novoId = conn.QuerySingle<int>(sqlProduto, new
                {
                    dados.Descricao,
                    dados.Tamanho,
                    dados.PrecoCusto,
                    dados.PrecoVenda,
                    dados.Origem,
                    dados.GrupoId,
                    StatusId = statusId,
                    dados.MarcaId,
                    dados.GeneroId,
                    dados.PerfilId,
                    dados.Condicao
                }, trans);

                var sqlEstoque = @"
                    INSERT INTO Estoque 
                    (Quantidade, Localizacao, DataAlteracao, ProdutoId, CodigoEstoque)
                    VALUES 
                    (1, 'Loja Principal', NOW(), @ProdutoId, @ProdutoId)";

                conn.Execute(sqlEstoque, new { ProdutoId = novoId }, trans);

                trans.Commit();
                return novoId;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }
    }
}