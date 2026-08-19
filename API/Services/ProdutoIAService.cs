using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABrechozeiraApp.Services;

/// <summary>
/// Modelo de produto extraído pela IA / Regras
/// </summary>
public class ProdutoImportado
{
    public string Descricao { get; set; } = "";
    public string? Tamanho { get; set; }
    public decimal PrecoVenda { get; set; }
    public int MarcaId { get; set; }
    public int GrupoId { get; set; }
    public int GeneroId { get; set; } = 1; // 1: Feminino, 2: Masculino, 3: Unissex
    public int PerfilId { get; set; } = 1; // 1: Adulto, 2: Infantil
    public string Condicao { get; set; } = "N";
}

/// <summary>
/// Serviço de inteligência para extrair campos estruturados de descrições de produtos.
/// Combina motor determinístico inteligente com Gemini / Claude AI.
/// </summary>
public class ProdutoIAService
{
    private readonly IConfiguration _configuration;
    private readonly CacheSistemaService _cacheSistema;
    private readonly ILogger<ProdutoIAService> _logger;

    public const int ID_MARCA_OUTROS = 10;
    public const int ID_GRUPO_OUTROS = 18;

    // Dicionário de Marcas e Abreviações / Apelidos
    private static readonly List<(int MarcaId, string CanonicalName, string[] Aliases)> BrandRegistry = new()
    {
        (1, "Tommy Hilfiger", new[] { "TH", "Tommy Hilfiger", "Tommy", "Hilfiger" }),
        (2, "GAP", new[] { "GAP" }),
        (3, "Michael Kors", new[] { "MK", "Michael Kors", "Michael", "Kors" }),
        (4, "Lança Perfume", new[] { "LP", "Lança Perfume", "Lanca Perfume", "Lanca" }),
        (5, "GUESS", new[] { "GUESS" }),
        (6, "Adidas", new[] { "Adidas" }),
        (7, "Nike", new[] { "Nike" }),
        (8, "Carmen Steffens", new[] { "CS", "Carmen Steffens", "Carmen", "Steffens" }),
        (9, "Levis", new[] { "Levis", "Levi's", "Levi" }),
        (10, "Outros", new[] { "Outros", "Outro" }),
        (11, "Felini", new[] { "Felini", "Feline" }),
        (12, "Animale", new[] { "Animale" }),
        (13, "Columbia", new[] { "Columbia" }),
        (14, "The North Face", new[] { "TNF", "The North Face", "North Face" }),
        (15, "Le Lis Blanc", new[] { "LLB", "Le Lis Blanc", "Le Lis", "Lis Blanc" }),
        (16, "Zara", new[] { "Zara" }),
        (17, "Farm", new[] { "Farm", "Farm Rio" })
    };

    // Dicionário de Grupos de Produtos
    private static readonly List<(int GrupoId, string[] Patterns)> GroupRegistry = new()
    {
        (1, new[] { "camiseta", "camisetas", "t-shirt", "t shirt", "tshirt", "t-shirts", "tee", "baby look", "babylook" }), // Camisetas
        (2, new[] { "camisa", "camisas", "camisaria", "polo", "camisa polo" }), // Camisas
        (3, new[] { "calca", "calça", "calças", "calcas", "jeans", "pantalona", "legging", "leggings", "pantacourt", "jogger", "flare", "skinny", "cargo", "wide leg", "mom jeans" }), // Calças
        (4, new[] { "short", "shorts", "bermuda", "bermudas", "shortinho", "ciclista" }), // Shorts e Bermudas
        (5, new[] { "vestido", "vestidos", "chemise", "tubinho" }), // Vestidos
        (6, new[] { "saia", "saias", "minissaia" }), // Saias
        (8, new[] { "jaqueta", "jaquetas", "parka", "corta vento", "corta-vento", "windbreaker", "bomber", "puffer", "anorak" }), // Jaquetas
        (9, new[] { "casaco", "casacos", "blazer", "blazers", "sobretudo", "trench coat", "moletom", "moletons", "hoodie" }), // Casacos
        (7, new[] { "blusa", "blusas", "bata", "regata", "regatas", "cropped", "body", "tricot", "trico", "tricô", "malha", "fleece", "fleeces", "ted", "teddy", "sueter", "suéter", "cardigan", "cardigã", "pullover" }), // Blusas
        (10, new[] { "top", "tops", "sutia", "sutiã", "bralette" }), // Tops
        (11, new[] { "calcinha", "cueca", "lingerie" }), // Roupas Íntimas
        (12, new[] { "pijama", "pijamas", "robe", "camisola" }), // Roupas de Dormir
        (13, new[] { "biquini", "biquíni", "biquinis", "maio", "maiô", "sunga", "saida de praia", "saída de praia" }), // Roupas de Banho
        (15, new[] { "bolsa", "bolsas", "clutch", "mochila", "mochilas", "carteira", "carteiras", "pochete", "necessaire" }), // Bolsas
        (16, new[] { "tenis", "tênis", "sapato", "sapatos", "sandalia", "sandália", "sandálias", "bota", "botas", "coturno", "scarpin", "mule", "rasteira", "rasteirinha", "chinelo", "chinelos", "sapatilha", "sapatilhas", "tamanco" }), // Calçados
        (14, new[] { "cinto", "cintos", "chapeu", "chapéu", "bone", "boné", "oculos", "óculos", "lenco", "lenço", "echarpe", "cachecol", "bijuteria", "colar", "brinco", "pulseira" }), // Acessórios
        (17, new[] { "brinquedo", "brinquedos", "boneca", "carrinho", "jogos" }) // Brinquedos
    };

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
    /// Processa a descrição com IA e Regras Heurísticas.
    /// Tenta Gemini -> Claude -> Motor Heurístico de Alta Precisão.
    /// </summary>
    public async Task<ProdutoImportado> ProcessarDescricaoAsync(string descricao, decimal precoVenda)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            return new ProdutoImportado
            {
                Descricao = "Produto sem descrição",
                PrecoVenda = precoVenda,
                MarcaId = ID_MARCA_OUTROS,
                GrupoId = ID_GRUPO_OUTROS,
                GeneroId = 1,
                PerfilId = 1,
                Condicao = "N"
            };
        }

        // Tenta IA se chave configurada
        var geminiKey = GetGeminiKey();
        var claudeKey = GetClaudeKey();

        if (!string.IsNullOrEmpty(geminiKey))
        {
            try
            {
                var resultado = await ProcessarComGeminiAsync(descricao, precoVenda);
                if (ValidarResultadoIA(resultado))
                    return resultado;
            }
            catch (Exception exGemini)
            {
                _logger.LogWarning("Gemini falhou para '{Descricao}': {Erro}", descricao, exGemini.Message);
            }
        }

        if (!string.IsNullOrEmpty(claudeKey))
        {
            try
            {
                var resultado = await ProcessarComClaudeAsync(descricao, precoVenda);
                if (ValidarResultadoIA(resultado))
                    return resultado;
            }
            catch (Exception exClaude)
            {
                _logger.LogWarning("Claude falhou para '{Descricao}': {Erro}", descricao, exClaude.Message);
            }
        }

        // Motor Determinístico / Heurístico de Regras
        return ExtrairPorRegras(descricao, precoVenda);
    }

    private bool ValidarResultadoIA(ProdutoImportado prod)
    {
        return prod != null && prod.MarcaId > 0 && prod.GrupoId > 0;
    }

    /// <summary>
    /// Motor Heurístico Inteligente de Extração por Regras e Abreviações
    /// </summary>
    public static ProdutoImportado ExtrairPorRegras(string descricaoOriginal, decimal precoVenda)
    {
        var raw = descricaoOriginal.Trim();
        var normalized = RemoverAcentos(raw).ToLowerInvariant();

        var resultado = new ProdutoImportado
        {
            Descricao = FormatarDescricao(raw),
            PrecoVenda = precoVenda,
            MarcaId = ID_MARCA_OUTROS,
            GrupoId = ID_GRUPO_OUTROS,
            GeneroId = 1, // Feminino padrão
            PerfilId = 1, // Adulto padrão
            Condicao = "N"
        };

        // 1. Extração de Marca (incluindo abreviações TH, MK, LP, CS, TNF, LLB, etc.)
        resultado.MarcaId = ExtrairMarcaId(raw, normalized);

        // 2. Extração de Grupo
        resultado.GrupoId = ExtrairGrupoId(normalized);

        // 3. Extração de Tamanho
        resultado.Tamanho = ExtrairTamanho(raw, normalized);

        // 4. Extração de Gênero
        resultado.GeneroId = ExtrairGeneroId(normalized, resultado.GrupoId);

        // 5. Extração de Perfil (Adulto vs Infantil)
        resultado.PerfilId = ExtrairPerfilId(normalized);

        return resultado;
    }

    private static int ExtrairMarcaId(string raw, string normalized)
    {
        // Ordem de busca: termos mais longos primeiro para evitar falsos positivos
        foreach (var brand in BrandRegistry)
        {
            if (brand.MarcaId == ID_MARCA_OUTROS) continue;

            foreach (var alias in brand.Aliases)
            {
                var aliasNorm = RemoverAcentos(alias).ToLowerInvariant();

                // Se a abreviação tem 2 ou 3 letras maiúsculas (TH, MK, LP, CS, TNF, LLB, GAP)
                if (alias.Length <= 4 && alias == alias.ToUpperInvariant())
                {
                    // Busca exata por palavra no texto original ou normalizado
                    var pattern = $@"\b{Regex.Escape(alias)}\b";
                    if (Regex.IsMatch(raw, pattern, RegexOptions.IgnoreCase))
                    {
                        return brand.MarcaId;
                    }
                }
                else
                {
                    var pattern = $@"\b{Regex.Escape(aliasNorm)}\b";
                    if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
                    {
                        return brand.MarcaId;
                    }
                }
            }
        }

        return ID_MARCA_OUTROS;
    }

    private static int ExtrairGrupoId(string normalized)
    {
        foreach (var group in GroupRegistry)
        {
            foreach (var patternWord in group.Patterns)
            {
                var normWord = RemoverAcentos(patternWord).ToLowerInvariant();
                var pattern = $@"\b{Regex.Escape(normWord)}\b";
                if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
                {
                    return group.GrupoId;
                }
            }
        }

        return ID_GRUPO_OUTROS;
    }

    private static string? ExtrairTamanho(string raw, string normalized)
    {
        // Padrão 1: "tam P", "tamanho M", "tam. 38", "nº 40"
        var matchPrefixo = Regex.Match(normalized, @"\b(?:tam|tamanho|n|no|num|numero)\.?\s*([0-9]{1,2}|pp|p|m|g|gg|xg|xgg|g1|g2|g3|u)\b", RegexOptions.IgnoreCase);
        if (matchPrefixo.Success)
        {
            return matchPrefixo.Groups[1].Value.ToUpperInvariant();
        }

        // Padrão 2: Tamanho de letras isolado "\b(PP|P|M|G|GG|XG|XGG|G1|G2|G3)\b"
        var matchLetra = Regex.Match(raw, @"\b(PP|P|M|G|GG|XG|XGG|G1|G2|G3)\b");
        if (matchLetra.Success)
        {
            return matchLetra.Groups[1].Value.ToUpperInvariant();
        }

        // Padrão 3: Numeração de calçados / calças no final da descrição: " 38", " 40"
        var matchNumeroFinal = Regex.Match(raw, @"\s+(3[4-9]|4[0-8]|5[0-2])\s*$");
        if (matchNumeroFinal.Success)
        {
            return matchNumeroFinal.Groups[1].Value;
        }

        return null;
    }

    private static int ExtrairGeneroId(string normalized, int grupoId)
    {
        if (Regex.IsMatch(normalized, @"\b(masculino|masculina|masc|homem|cueca|sunga)\b"))
            return 2; // Masculino

        if (Regex.IsMatch(normalized, @"\b(unissex|unisex)\b"))
            return 3; // Unissex

        if (Regex.IsMatch(normalized, @"\b(feminino|feminina|fem|mulher|vestido|saia|cropped|sutia|biquini|calcinha|sandalia|rasteira|tamanco|scarpin|bata)\b"))
            return 1; // Feminino

        // Grupos essencialmente femininos
        if (grupoId == 5 || grupoId == 6 || grupoId == 10) // Vestidos, Saias, Tops
            return 1;

        return 1; // Padrão da loja
    }

    private static int ExtrairPerfilId(string normalized)
    {
        if (Regex.IsMatch(normalized, @"\b(infantil|kids|menina|menino|bebe|bebezinha|bebezinho|anos|meses|0-3m|3-6m|6-9m|9-12m|1 ano|2 anos|3 anos|4 anos|6 anos|8 anos|10 anos|12 anos|14 anos)\b"))
            return 2; // Infantil

        return 1; // Adulto padrão
    }

    private static string FormatarDescricao(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        // Capitaliza primeira letra de cada palavra relevante ou limpa espaços extras
        raw = Regex.Replace(raw.Trim(), @"\s+", " ");
        return raw;
    }

    private static string RemoverAcentos(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private string MontarPrompt(string descricao, decimal precoVenda)
    {
        var jsonMarcas = JsonSerializer.Serialize(_cacheSistema.Marcas);
        var jsonGrupos = JsonSerializer.Serialize(_cacheSistema.Grupos);

        return $@"Você é um especialista em cadastro de produtos de brechó de moda.
                
--- TABELAS DE DOMÍNIO ---
MARCAS DISPONÍVEIS:
- Tommy Hilfiger (Id: 1) [Abreviações: TH, Tommy]
- GAP (Id: 2)
- Michael Kors (Id: 3) [Abreviações: MK, Kors]
- Lança Perfume (Id: 4) [Abreviações: LP, Lanca Perfume]
- GUESS (Id: 5)
- Adidas (Id: 6)
- Nike (Id: 7)
- Carmen Steffens (Id: 8) [Abreviações: CS]
- Levis (Id: 9) [Abreviações: Levi's, Levi]
- Outros (Id: 10)
- Felini (Id: 11)
- Animale (Id: 12)
- Columbia (Id: 13)
- The North Face (Id: 14) [Abreviações: TNF]
- Le Lis Blanc (Id: 15) [Abreviações: LLB, Le Lis]
- Zara (Id: 16)
- Farm (Id: 17) [Abreviações: Farm Rio]

GRUPOS DISPONÍVEIS:
1: Camisetas (t-shirt, tee, camiseta)
2: Camisas (social, polo)
3: Calças (jeans, calça, legging, pantalona)
4: Shorts e Bermudas
5: Vestidos
6: Saias
7: Blusas (bata, regata, cropped, tricot, body, fleece, ted, teddy, suéter, cardigan)
8: Jaquetas (parka, corta vento, puffer)
9: Casacos (blazer, moletom, casaco, hoodie, sobretudo)
10: Tops (sutiã, top)
11: Roupas Íntimas
12: Roupas de Dormir
13: Roupas de Banho
14: Acessórios (cinto, óculos, boné)
15: Bolsas
16: Calçados (tênis, sapato, sandália, bota)
17: Brinquedos
18: Outros

DESCRIÇÃO: '{descricao}'
PREÇO: {precoVenda}

REGRAS OBRIGATÓRIAS:
1. Identifique a Marca mesmo se estiver abreviada (ex: 'TH' -> 1, 'LP' -> 4, 'MK' -> 3, 'CS' -> 8, 'TNF' -> 14, 'LLB' -> 15, 'zara' -> 16, 'columbia' -> 13).
2. Identifique o Grupo correspondente (ex: 'camiseta' -> 1, 'jaqueta' -> 8, 'moletom' -> 9, 'vestido' -> 5).
3. Identifique o Tamanho (P, M, G, GG, 38, 40, etc) ou null.
4. Gênero: 1 (Feminino), 2 (Masculino), 3 (Unissex). Padrão 1.
5. Perfil: 1 (Adulto), 2 (Infantil). Padrão 1.

RETORNE APENAS ESTE JSON (sem markdown, sem explicações):
{{
    ""Descricao"": ""{descricao}"",
    ""Tamanho"": null,
    ""MarcaId"": 16,
    ""GrupoId"": 1,
    ""GeneroId"": 1,
    ""PerfilId"": 1,
    ""Condicao"": ""N""
}}";
    }

    private ProdutoImportado ParsearResposta(string text, decimal precoVenda)
    {
        if (string.IsNullOrEmpty(text))
            throw new Exception("IA retornou vazio.");

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

        if (dados.MarcaId == 0) dados.MarcaId = ID_MARCA_OUTROS;
        if (dados.GrupoId == 0) dados.GrupoId = ID_GRUPO_OUTROS;
        if (dados.GeneroId == 0) dados.GeneroId = 1;
        if (dados.PerfilId == 0) dados.PerfilId = 1;

        return dados;
    }

    private async Task<ProdutoImportado> ProcessarComGeminiAsync(string descricao, decimal precoVenda)
    {
        var apiKey = GetGeminiKey();
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
                temperature = 0.2
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

    private async Task<ProdutoImportado> ProcessarComClaudeAsync(string descricao, decimal precoVenda)
    {
        var apiKey = GetClaudeKey();
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
