using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace ABrechozeiraApp.Services;

/// <summary>
/// Representa uma linha de arremate lida da planilha
/// </summary>
public class LinhaArremate
{
    public int? CodigoLive { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Comprador { get; set; } = string.Empty;
    public string Fila { get; set; } = string.Empty;
    public int LinhaOriginal { get; set; }
}

/// <summary>
/// Serviço para leitura de planilhas Google Sheets (via URL/CSV) e arquivos .xlsx
/// </summary>
public class GoogleSheetReaderService
{
    private readonly ILogger<GoogleSheetReaderService> _logger;

    public GoogleSheetReaderService(ILogger<GoogleSheetReaderService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lê dados da aba "vendas" de uma Google Sheet via URL pública
    /// </summary>
    public async Task<List<LinhaArremate>> LerPorUrlAsync(string googleSheetUrl, string sheetName = "vendas")
    {
        // Extrair o spreadsheet ID da URL
        var match = Regex.Match(googleSheetUrl, @"/d/([a-zA-Z0-9_-]+)");
        if (!match.Success)
            throw new ArgumentException("URL da planilha inválida. Esperado formato: https://docs.google.com/spreadsheets/d/ID_DA_PLANILHA/...");

        var spreadsheetId = match.Groups[1].Value;
        var csvUrl = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/gviz/tq?tqx=out:csv&sheet={Uri.EscapeDataString(sheetName)}";

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        var response = await client.GetAsync(csvUrl);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erro ao baixar planilha: HTTP {response.StatusCode}");

        var csvContent = await response.Content.ReadAsStringAsync();
        return ParsearCsv(csvContent);
    }

    /// <summary>
    /// Lê dados de um arquivo .xlsx (aba "vendas" ou primeira aba)
    /// </summary>
    public List<LinhaArremate> LerPorXlsx(Stream stream, string sheetName = "vendas")
    {
        using var workbook = new XLWorkbook(stream);

        // Tentar encontrar a aba "vendas", senão usar a primeira
        var worksheet = workbook.Worksheets.FirstOrDefault(w =>
            w.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheet(1);

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        var linhas = new List<LinhaArremate>();

        // Pula a linha 1 (cabeçalho)
        for (int row = 2; row <= lastRow; row++)
        {
            try
            {
                var descricao = worksheet.Cell(row, 3).GetString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(descricao)) continue;

                var codigoStr = worksheet.Cell(row, 1).GetString()?.Trim() ?? "";
                int? codigoLive = null;
                if (int.TryParse(codigoStr, out var codParsed))
                    codigoLive = codParsed;

                var valorStr = worksheet.Cell(row, 4).GetString()?.Trim() ?? "0";
                decimal valor = 0;
                decimal.TryParse(valorStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out valor);

                var comprador = worksheet.Cell(row, 5).GetString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(comprador)) continue;

                // Fila: colunas 6 em diante
                var fila = new List<string>();
                for (int col = 6; col <= 17; col++)
                {
                    var val = worksheet.Cell(row, col).GetString()?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(val))
                        fila.Add(val);
                }

                linhas.Add(new LinhaArremate
                {
                    CodigoLive = codigoLive,
                    Descricao = descricao,
                    Valor = valor,
                    Comprador = comprador,
                    Fila = string.Join(", ", fila),
                    LinhaOriginal = row
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao ler linha {Row} do XLSX: {Erro}", row, ex.Message);
            }
        }

        _logger.LogInformation("XLSX: {Count} linhas lidas de '{Sheet}'", linhas.Count, worksheet.Name);
        return linhas;
    }

    /// <summary>
    /// Parseia o conteúdo CSV da Google Sheet
    /// </summary>
    private List<LinhaArremate> ParsearCsv(string csvContent)
    {
        var linhas = new List<LinhaArremate>();
        var rows = ParseCsvRows(csvContent);

        // Pula a primeira linha (cabeçalho)
        for (int i = 1; i < rows.Count; i++)
        {
            try
            {
                var cols = rows[i];
                if (cols.Count < 5) continue;

                var descricao = cols.Count > 2 ? cols[2].Trim() : "";
                if (string.IsNullOrWhiteSpace(descricao)) continue;

                var codigoStr = cols[0].Trim();
                int? codigoLive = null;
                if (int.TryParse(codigoStr, out var codParsed))
                    codigoLive = codParsed;

                var valorStr = cols.Count > 3 ? cols[3].Trim() : "0";
                decimal valor = 0;
                decimal.TryParse(valorStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out valor);

                var comprador = cols.Count > 4 ? cols[4].Trim() : "";
                if (string.IsNullOrWhiteSpace(comprador)) continue;

                // Fila: colunas 5 em diante (índice 5+)
                var fila = new List<string>();
                for (int col = 5; col < cols.Count; col++)
                {
                    var val = cols[col].Trim();
                    if (!string.IsNullOrWhiteSpace(val))
                        fila.Add(val);
                }

                linhas.Add(new LinhaArremate
                {
                    CodigoLive = codigoLive,
                    Descricao = descricao,
                    Valor = valor,
                    Comprador = comprador,
                    Fila = string.Join(", ", fila),
                    LinhaOriginal = i + 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao parsear linha CSV {Row}: {Erro}", i + 1, ex.Message);
            }
        }

        _logger.LogInformation("CSV: {Count} linhas parseadas", linhas.Count);
        return linhas;
    }

    /// <summary>
    /// Parser de CSV que lida com campos entre aspas contendo vírgulas
    /// </summary>
    private List<List<string>> ParseCsvRows(string csv)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Checar aspas duplas (escape)
                    if (i + 1 < csv.Length && csv[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; // pular próxima aspa
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                }
                else if (c == '\n' || (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n'))
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    if (currentRow.Any(f => !string.IsNullOrWhiteSpace(f)))
                        rows.Add(currentRow);
                    currentRow = new List<string>();
                    if (c == '\r') i++; // pular \n
                }
                else
                {
                    currentField.Append(c);
                }
            }
        }

        // Última linha
        currentRow.Add(currentField.ToString());
        if (currentRow.Any(f => !string.IsNullOrWhiteSpace(f)))
            rows.Add(currentRow);

        return rows;
    }
}
