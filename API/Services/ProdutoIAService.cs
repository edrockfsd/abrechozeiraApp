using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABrechozeiraApp.Services;

/// <summary>
/// Modelo de produto extraído pela IA
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

/// <summary>
/// Serviço de IA para extrair campos estruturados de descrições de produtos.
/// Usa Gemini como primário e Claude (Haiku) como fallback.
/// </summary>
public class ProdutoIAService
{
    private readonly IConfiguration _configuration;
    private readonly CacheSistemaService _cacheSistema;
    private readonly ILogger<ProdutoIAService> _logger;

    private const int ID_MARCA_GENERICA = 10;
    private const int ID_GRUPO_GENERICO = 18;

    public ProdutoIAService(
        IConfiguration configuration,
        CacheSistemaService cacheSistema,
        ILogger<ProdutoIAService> logger)
    {
        _configuration = configuration;
        _cacheSistema = cacheSistema;
        _logger = logger;
    }

    private string GetGeminiKey() => _configuration["Gemini:ApiKey"] ?? "";
    private string GetClaudeKey() => _configuration["Claude:ApiKey"] ?? "";

    /// <summary>
    /// Processa a descrição com IA para extrair campos estruturados.
    /// Tenta Gemini primeiro; se falhar, usa Claude como fallback.
    /// </summary>
    public async Task<ProdutoImportado> ProcessarDescricaoAsync(string descricao, decimal precoVenda)
    {
        try
        {
            var resultado = await ProcessarComGeminiAsync(descricao, precoVenda);
            return resultado;
        }
        catch (Exception exGemini)
        {
            _logger.LogWarning("Gemini falhou para '{Descricao}': {Erro}. Tentando Claude...", descricao, exGemini.Message);

            try
            {
                var resultado = await ProcessarComClaudeAsync(descricao, precoVenda);
                return resultado;
            }
            catch (Exception exClaude)
            {
                _logger.LogError("Claude também falhou para '{Descricao}': {Erro}. Usando valores padrão.", descricao, exClaude.Message);

                // Fallback final: valores padrão
                return new ProdutoImportado
                {
                    Descricao = descricao,
                    PrecoVenda = precoVenda,
                    MarcaId = ID_MARCA_GENERICA,
                    GrupoId = ID_GRUPO_GENERICO,
                    GeneroId = 1,
                    PerfilId = 1,
                    Condicao = "N"
                };
            }
        }
    }

    private string MontarPrompt(string descricao, decimal precoVenda)
    {
        var jsonMarcas = JsonSerializer.Serialize(_cacheSistema.Marcas);
        var jsonGrupos = JsonSerializer.Serialize(_cacheSistema.Grupos);
        var jsonGeneros = JsonSerializer.Serialize(_cacheSistema.Generos);
        var jsonPerfis = JsonSerializer.Serialize(_cacheSistema.Perfis);

        return $@"Você é um especialista em cadastro de produtos de brechó.
                
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
    }

    private ProdutoImportado ParsearResposta(string text, decimal precoVenda)
    {
        if (string.IsNullOrEmpty(text))
            throw new Exception("IA retornou vazio.");

        // Limpeza de markdown
        text = text.Replace("```json", "").Replace("```", "").Trim();
        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        if (jsonStart != -1 && jsonEnd != -1)
        {
            text = text.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dados = JsonSerializer.Deserialize<ProdutoImportado>(text, opts) ?? new ProdutoImportado();
        dados.PrecoVenda = precoVenda;

        // Validar IDs — aplicar defaults se inválidos
        if (dados.MarcaId == 0) dados.MarcaId = ID_MARCA_GENERICA;
        if (dados.GrupoId == 0) dados.GrupoId = ID_GRUPO_GENERICO;
        if (dados.GeneroId == 0) dados.GeneroId = 1;
        if (dados.PerfilId == 0) dados.PerfilId = 1;

        return dados;
    }

    /// <summary>
    /// Processa com Google Gemini (gemma-3-27b-it)
    /// </summary>
    private async Task<ProdutoImportado> ProcessarComGeminiAsync(string descricao, decimal precoVenda)
    {
        var apiKey = GetGeminiKey();
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("Chave Gemini não configurada.");

        using var client = new HttpClient();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemma-3-27b-it:generateContent?key={apiKey}";

        var prompt = MontarPrompt(descricao, precoVenda);

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

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erro Gemini: {responseString}");

        using var jsonDoc = JsonDocument.Parse(responseString);
        var text = jsonDoc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString();

        return ParsearResposta(text!, precoVenda);
    }

    /// <summary>
    /// Processa com Claude (Haiku - modelo mais simples/barato) como fallback
    /// </summary>
    private async Task<ProdutoImportado> ProcessarComClaudeAsync(string descricao, decimal precoVenda)
    {
        var apiKey = GetClaudeKey();
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("Chave Claude não configurada.");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var prompt = MontarPrompt(descricao, precoVenda);

        var body = new
        {
            model = "claude-haiku-4-20250414",
            max_tokens = 500,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erro Claude: {responseString}");

        using var jsonDoc = JsonDocument.Parse(responseString);
        var text = jsonDoc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text").GetString();

        return ParsearResposta(text!, precoVenda);
    }
}
