using AbiaApp.API.Services;
using ClosedXML.Excel;
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
    public class ImportController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private const int ID_MARCA_GENERICA = 10;
        private const int ID_GRUPO_GENERICO = 18;

        public ImportController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() => _configuration.GetConnectionString("DefaultConnection");
        private string GetOpenAIKey() => _configuration["OpenAI:ApiKey"];
        private string GetGeminiKey() => _configuration["Gemini:ApiKey"];

        /// <summary>
        /// Upload de Excel para importação em massa
        /// Colunas esperadas: CodigoEstoque, Descricao, PrecoVenda
        /// </summary>
        [HttpPost("excel")]
        public async Task<IActionResult> ImportarExcel(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest(new { erro = "Nenhum arquivo enviado." });

            var resultados = new List<object>();
            var erros = new List<object>();
            int sucessos = 0;

            try
            {
                using var stream = arquivo.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                // Encontra a última linha com dados
                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                // Começa da linha 2 (assume cabeçalho na linha 1)
                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var codigoEstoque = worksheet.Cell(row, 1).GetValue<int>();
                        var descricao = worksheet.Cell(row, 2).GetString()?.Trim();
                        var precoVenda = worksheet.Cell(row, 3).GetValue<decimal>();

                        if (string.IsNullOrEmpty(descricao))
                        {
                            erros.Add(new { linha = row, erro = "Descrição vazia" });
                            continue;
                        }

                        // Processa a descrição com IA para extrair campos
                        var dadosExtraidos = await ProcessarDescricaoComIA(descricao, precoVenda);
                        
                        // Salva no banco
                        var idNovo = SalvarNoBanco(dadosExtraidos, codigoEstoque);

                        sucessos++;
                        resultados.Add(new
                        {
                            linha = row,
                            codigoEstoque,
                            descricao = dadosExtraidos.Descricao,
                            precoVenda = dadosExtraidos.PrecoVenda,
                            marca = dadosExtraidos.MarcaId,
                            grupo = dadosExtraidos.GrupoId,
                            tamanho = dadosExtraidos.Tamanho,
                            idBanco = idNovo
                        });
                    }
                    catch (Exception ex)
                    {
                        erros.Add(new { linha = row, erro = ex.Message });
                    }
                }

                return Ok(new
                {
                    mensagem = $"Importação concluída: {sucessos} produtos cadastrados",
                    totalLinhas = lastRow - 1,
                    sucessos,
                    erros = erros.Count,
                    detalhes = resultados,
                    listaErros = erros
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = $"Erro ao processar arquivo: {ex.Message}" });
            }
        }

        /// <summary>
        /// Processa a descrição com IA para extrair campos estruturados (Gemini)
        /// </summary>
        private async Task<ProdutoImportado> ProcessarDescricaoComIA(string descricao, decimal precoVenda)
        {
            using var client = new HttpClient();
            var apiKey = GetGeminiKey();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemma-3-27b-it:generateContent?key={apiKey}";

            var jsonMarcas = JsonConvert.SerializeObject(CacheSistema.Marcas);
            var jsonGrupos = JsonConvert.SerializeObject(CacheSistema.Grupos);
            var jsonGeneros = JsonConvert.SerializeObject(CacheSistema.Generos);
            var jsonPerfis = JsonConvert.SerializeObject(CacheSistema.Perfis);

            var prompt = $@"Você é um especialista em cadastro de produtos de brechó.
                
--- TABELAS DE DOMÍNIO ---
MARCAS: {jsonMarcas}
GRUPOS: {jsonGrupos}
GÊNEROS: {jsonGeneros}
PERFIS: {jsonPerfis}
--------------------------

DESCRIÇÃO DO PRODUTO: '{descricao}'
PREÇO DE VENDA: {precoVenda}

MISSÃO: Extrair e estruturar os dados da descrição.

REGRAS:
1. Encontre a Marca mais próxima na lista (fuzzy match). Se não encontrar, use {ID_MARCA_GENERICA}.
2. Encontre o Grupo/Categoria mais próximo. Se não encontrar, use {ID_GRUPO_GENERICO}.
3. Identifique o Gênero (masculino/feminino/unissex) pelo contexto.
4. Identifique o Perfil (adulto/infantil) pelo contexto.
5. Extraia o Tamanho se mencionado (P, M, G, GG, 38, 40, etc).
6. Condicao: 'N' (Novo) ou 'U' (Usado). Padrão 'N'.

RETORNE APENAS O JSON (sem explicações, sem markdown ```json):
{{
    ""Descricao"": ""descrição limpa e formatada"",
    ""Tamanho"": ""string ou null"",
    ""MarcaId"": 0,
    ""GrupoId"": 0,
    ""GeneroId"": 1,
    ""PerfilId"": 1,
    ""Condicao"": ""N""
}}";

            var body = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    temperature = 0.3
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro Gemini: {responseString}");

            var jsonResponse = JToken.Parse(responseString);
            var text = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            if (string.IsNullOrEmpty(text))
                throw new Exception("IA retornou vazio.");

            // Limpeza manual para o Gemma (que não tem JSON Mode nativo)
            text = text.Replace("```json", "").Replace("```", "").Trim();
            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');
            if (jsonStart != -1 && jsonEnd != -1)
            {
                text = text.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            var dados = JsonConvert.DeserializeObject<ProdutoImportado>(text) ?? new ProdutoImportado();
            dados.PrecoVenda = precoVenda;
            
            return dados;
        }

        /// <summary>
        /// Salva o produto no banco
        /// </summary>
        private int SalvarNoBanco(ProdutoImportado dados, int codigoEstoque)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                if (dados.MarcaId == 0) dados.MarcaId = ID_MARCA_GENERICA;
                if (dados.GrupoId == 0) dados.GrupoId = ID_GRUPO_GENERICO;
                if (dados.GeneroId == 0) dados.GeneroId = 1;
                if (dados.PerfilId == 0) dados.PerfilId = 1;

                var sqlProduto = @"
                    INSERT INTO Produto 
                    (Descricao, Tamanho, PrecoCusto, PrecoVenda, Origem, 
                     GrupoID, DataCompra, DataAlteracao, StatusId, 
                     MarcaId, GeneroID, PerfilID, Condicao)
                    VALUES 
                    (@Descricao, @Tamanho, 0, @PrecoVenda, 'Excel Import', 
                     @GrupoId, NOW(), NOW(), 1, 
                     @MarcaId, @GeneroId, @PerfilId, @Condicao);
                    SELECT LAST_INSERT_ID();";

                int novoId = conn.QuerySingle<int>(sqlProduto, new
                {
                    dados.Descricao,
                    dados.Tamanho,
                    dados.PrecoVenda,
                    dados.GrupoId,
                    dados.MarcaId,
                    dados.GeneroId,
                    dados.PerfilId,
                    dados.Condicao
                }, trans);

                var sqlEstoque = @"
                    INSERT INTO Estoque 
                    (Quantidade, Localizacao, DataAlteracao, ProdutoId, CodigoEstoque)
                    VALUES 
                    (1, 'Loja Principal', NOW(), @ProdutoId, @CodigoEstoque)";

                conn.Execute(sqlEstoque, new { ProdutoId = novoId, CodigoEstoque = codigoEstoque }, trans);

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

    /// <summary>
    /// Modelo para produto importado do Excel
    /// </summary>
    public class ProdutoImportado
    {
        public string Descricao { get; set; } = "";
        public string? Tamanho { get; set; }
        public decimal PrecoVenda { get; set; }
        public int MarcaId { get; set; }
        public int GrupoId { get; set; }
        public int GeneroId { get; set; } = 1;
        public int PerfilId { get; set; } = 1;
        public string Condicao { get; set; } = "N";
    }
}
