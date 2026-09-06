using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ABrechozeiraApp.DTOs;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Services
{
    public class GoogleSheetsSyncService
    {
        private const string CredencialEmbutidaB64 = "ewogICJ0eXBlIjogInNlcnZpY2VfYWNjb3VudCIsCiAgInByb2plY3RfaWQiOiAiYnJlY2hvLWxpdmUtdHJhY2tlciIsCiAgInByaXZhdGVfa2V5X2lkIjogIjFlZDZlNjkzNzU2ZjNmZDNiMGM2N2Y3Zjg2YjdjYjg3MmFkNWVhYmUiLAogICJwcml2YXRlX2tleSI6ICItLS0tLUJFR0lOIFBSSVZBVEUgS0VZLS0tLS1cbk1JSUV2QUlCQURBTkJna3Foa2lHOXcwQkFRRUZBQVNDQktZd2dnU2lBZ0VBQW9JQkFRREFyOUxpalhwRWc2RTBcbnh5Sk1MbHNiSnExZ0Y0elA5VlBnVDFRZlJyWEszekZwSHdmdEZLWGt4TU41NkRoZk53b3lkZHZxc0FrdUs1N3RcbkhCSE5RYjBjVU05WTlEbkNLS2FMeFFJcHZCWXIxbUZHSEhKeWl0ZEJLNW1lRGZXV2J4dEJHaUpOQi9hQmVGYnRcbnp0RUNLNlJtQTRPYTFXd01NZjNCVTBpbjBldCs2ZHR1T1N5dlZkOVpmRU0vdDZ3bWtRaXVkUHlmZzJpU05yNzhcbmpzOEtmMUVnRjBYSGI3WUxFMytEUnV3MFdNUDVNcVFxWFl0anlwV0h0bmRTUWpIUk5TWmc4dUF1eVlvazBmS1ZcbkdTbHRJdGVwTXBpVHpOTGdrSW5KVzVxYXF1c1NNMTB1NjhvaUIyL2xJNU5kbC9mMHVOM3RTS0hmb3hrZWtIRlpcblJSbW5XZlpmQWdNQkFBRUNnZ0VBQWtZUTNGRFp1eXFTZHhKTkpVNVJaT1gxUmNlc0J3Z3VEOXpmeHBRSnNTQjdcbnhNY2VTMzNpNGVyYkk2VWZMOVhHYmx5ckFQOUlISE5jcDJUeXUzNmx3ZlVjMHBVNTJVQ3NObTlYWElhMlVaakdcbkFtaHpocHZTSThLNm5WTFhnd21YUTJnUTM0MG9nTlB3Qi81WXc3TWdJdFFIeVlhYkw4bW5PanR4V3JSRXd4L3Zcblpta2pvb1R2SmppUEZKa3daL0FLOHBVY2Q4S0dsSkU2b1pVUlJzMU1rSmJtR01Ud1N6cmtmMFdBdlJvSzNlWHZcbnRsRWZvNUdyRGozOHUvVFBETmJyS0tOTUh2RS9IdU9vZkgwelFvdjN5UUJodEFYZmI1cG1xaE9VUHNaa3djV1NcbnlSKyttRlJZRjN5b0x4bXU0MTkzYUdPdDg4a1V2Znd3dFlCMVQ3N0NBUUtCZ1FEc2o4QXVKaHNhTm9ncHJDQVZcbklabWgrbEFISXB1TlliTG1YWDQzNGVvWnVvVlVBd294OGZjbjA2bXJrU3JjejV6bUl0RmYwVUdVeUhNT0d5M0lcbnVKZGZzSGJpUVhtNnhXT0ZKZ3R4OGNYUllFdzVvb2RTMkxSWU5IcUhiTm52WFVrMWdqaEJ4bWhVWXZSK1JyOUJcbmVSYUJrOG0vQ3lLYVRsUDZNWlFhd0RjRVVRS0JnUURRaFNLd2htU1hFa2NXL0kwYkQyazBabkV3eVNmMjJ2Tldcbnp0Vmh5M3RpZDlnZUE5VkdQOGV3SU5QNjY0Y1NiL0NUSWNEdC9jeU1RSzhCU2YxWXFTMnFsb00rZ255VzFNMFdcbjMrNEZQNWRsdmtCbVQ5MzBnZlNhQjhjbDBnVEhrYmMyT3Q3aGZmVjZkZ1RSbzNoR2R1SGhzYUhjMC9CdVdhTk1cbk9QWE5acXdUcndLQmdBdEFSd2FxMzAwZTNOa0dpN3dWamdZTExyVTRLeWZOUnNINEhtR3dCLzlUUkxZYk11ZnZcbk55OFl2UkFnNW1YOEpkMDRPTGNTNzhpUXhCQWVzTTNFSWNiMlVLRXdOZ2J4RG11dHhJdVYwUy9GSWJyNEJQR21cbnh1VVBFZWpRLzRpSDZreDJOaERDekFGL2QxdHVKL2lJTnM3UTVaNkZmQTdVdk4vQWJlZmJ6b0lSQW9HQUFLVllcbjdTa05hTFppeC94eVIzSXR4ajdHL1BxeWgvNDNvTDQxUlA5SFc0KzVlS1pVWThwUXlRZmhBRGI0alNNUm9MTUxcbkVQR25UNHZudnQ1R3paNkFpTFc4cEZYdEl0NTM0Q2xEYUhyQThreTdrRldRLzIvam5SS0hHR1BsSmVVYXB5MVpcbmdXTk5mbHFkT3pVQmExNTA1cWtSTWtqcHhyMjZWc01hWjg5NG12c0NnWUFpUnBWWDhrRnE5Rzc4Njh5ODE2TjNcbktyNlRDNHhwN2N6SGFRZjFheTlLRElNeFFHd1p1NUNUY1Y4VjZPenJPN0NFWVJ4M2tzcytjcW90TlJ2dWNpTDlcbkhpQVh3cFR4ZWlvOHNYYXVjM1orWktlTG5IRG9LQ1dwVzByRWpISGlDQkIxUGtGbEFhZnJDV21oYkFPQTVkeEhcblg1NXpWMmRVRjB4MWg0dWpocnhBeGc9PVxuLS0tLS1FTkQgUFJJVkFURSBLRVktLS0tLVxuIiwKICAiY2xpZW50X2VtYWlsIjogImxpdmVzLWNvbnRyb2xlQGJyZWNoby1saXZlLXRyYWNrZXIuaWFtLmdzZXJ2aWNlYWNjb3VudC5jb20iLAogICJjbGllbnRfaWQiOiAiMTA0MDY0OTE0NTMwMTM3NzM1Mzg5IiwKICAiYXV0aF91cmkiOiAiaHR0cHM6Ly9hY2NvdW50cy5nb29nbGUuY29tL28vb2F1dGgyL2F1dGgiLAogICJ0b2tlbl91cmkiOiAiaHR0cHM6Ly9vYXV0aDIuZ29vZ2xlYXBpcy5jb20vdG9rZW4iLAogICJhdXRoX3Byb3ZpZGVyX3g1MDlfY2VydF91cmwiOiAiaHR0cHM6Ly93d3cuZ29vZ2xlYXBpcy5jb20vb2F1dGgyL3YxL2NlcnRzIiwKICAiY2xpZW50X3g1MDlfY2VydF91cmwiOiAiaHR0cHM6Ly93d3cuZ29vZ2xlYXBpcy5jb20vcm9ib3QvdjEvbWV0YWRhdGEveDUwOS9saXZlcy1jb250cm9sZSU0MGJyZWNoby1saXZlLXRyYWNrZXIuaWFtLmdzZXJ2aWNlYWNjb3VudC5jb20iLAogICJ1bml2ZXJzZV9kb21haW4iOiAiZ29vZ2xlYXBpcy5jb20iCn0K";

        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleSheetsSyncService> _logger;

        public GoogleSheetsSyncService(IConfiguration configuration, ILogger<GoogleSheetsSyncService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string? LocalizarArquivoCredenciais()
        {
            var caminhos = new[]
            {
                _configuration["GoogleSheets:CredentialsPath"],
                Path.Combine(AppContext.BaseDirectory, "credentials", "service-account.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "credentials", "service-account.json"),
                @"C:\Users\eduar\GIT_REPOS\brecho-live-tracker\credentials\service-account.json"
            };

            foreach (var path in caminhos)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private SheetsService ObterSheetsService()
        {
            GoogleCredential credential;

            // 1. Tentar por JSON configurado
            var configJson = _configuration["GoogleSheets:CredentialsJson"];
            if (!string.IsNullOrWhiteSpace(configJson))
            {
                credential = GoogleCredential.FromJson(configJson).CreateScoped(SheetsService.Scope.Spreadsheets);
            }
            else
            {
                // 2. Tentar por arquivo no disco
                var credPath = LocalizarArquivoCredenciais();
                if (credPath != null)
                {
                    using var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read);
                    credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.Spreadsheets);
                }
                else
                {
                    // 3. Fallback: Credencial padrão embutida (Base64)
                    var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(CredencialEmbutidaB64));
                    credential = GoogleCredential.FromJson(json).CreateScoped(SheetsService.Scope.Spreadsheets);
                }
            }

            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ABrechozeira Live Tracker"
            });
        }

        private string? ExtrairSpreadsheetId(string urlOuId)
        {
            if (string.IsNullOrWhiteSpace(urlOuId)) return null;

            var match = Regex.Match(urlOuId, @"/d/([a-zA-Z0-9_-]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Se o usuário passou diretamente o ID
            if (urlOuId.Length > 20 && !urlOuId.Contains("/"))
            {
                return urlOuId.Trim();
            }

            return null;
        }

        /// <summary>
        /// Garante que a planilha possui o cabeçalho completo
        /// </summary>
        public async Task GarantirCabecalhoAsync(SheetsService service, string spreadsheetId, string sheetName)
        {
            try
            {
                var checkRange = $"{sheetName}!A1:H1";
                var getRequest = service.Spreadsheets.Values.Get(spreadsheetId, checkRange);
                var response = await getRequest.ExecuteAsync();

                if (response.Values == null || response.Values.Count == 0)
                {
                    var cabecalho = new List<object>
                    {
                        "Externo", "Intern", "Descricao", "Valor",
                        "Comprador",
                        "Fila 1", "Fila 2", "Fila 3", "Fila 4", "Fila 5"
                    };

                    var valueRange = new ValueRange { Values = new List<IList<object>> { cabecalho } };
                    var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"{sheetName}!A1");
                    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                    await updateRequest.ExecuteAsync();

                    _logger.LogInformation("Cabeçalho padrão criado na planilha {SpreadsheetId}, aba {SheetName}", spreadsheetId, sheetName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Não foi possível verificar/criar cabeçalho na planilha: {Erro}", ex.Message);
            }
        }

        /// <summary>
        /// Adiciona uma linha na planilha do Google Sheets com todos os dados da peça, comprador e fila em tempo real
        /// </summary>
        public async Task<bool> SincronizarPecaNaPlanilhaAsync(ConfirmarArremateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GoogleSheetUrl))
            {
                _logger.LogInformation("URL da Google Sheet não informada para a peça {Codigo}. Sincronização ignorada.", request.Codigo);
                return false;
            }

            var spreadsheetId = ExtrairSpreadsheetId(request.GoogleSheetUrl);
            if (string.IsNullOrEmpty(spreadsheetId))
            {
                _logger.LogWarning("Não foi possível extrair o ID da planilha da URL: {Url}", request.GoogleSheetUrl);
                return false;
            }

            try
            {
                var service = ObterSheetsService();
                var sheetName = string.IsNullOrWhiteSpace(request.SheetName) ? "vendas" : request.SheetName;

                await GarantirCabecalhoAsync(service, spreadsheetId, sheetName);

                // Montar a linha:
                // [Externo, Intern, Descricao, Valor, Comprador, Fila 1, Fila 2, Fila 3, ...]
                var row = new List<object>
                {
                    request.Codigo,
                    "", // Intern
                    request.Descricao,
                    request.Valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    request.Arrematante ?? ""
                };

                // Fila (Posição 2 em diante): SOMENTE OS NOMES DE USUÁRIO
                var filaMatches = request.Matches
                    .Where(m => m.Posicao > 1)
                    .OrderBy(m => m.Posicao)
                    .ToList();

                foreach (var fila in filaMatches)
                {
                    row.Add(fila.Username);
                }

                var valueRange = new ValueRange
                {
                    Values = new List<IList<object>> { row }
                };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, $"{sheetName}!A:Z");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                var result = await appendRequest.ExecuteAsync();
                _logger.LogInformation("Peça {Codigo} sincronizada com sucesso no Google Sheets: {Updates}", request.Codigo, result.Updates?.UpdatedRange);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar peça {Codigo} no Google Sheets: {Erro}", request.Codigo, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sincroniza uma lista de arremates já salvos no banco de dados para a planilha Google
        /// </summary>
        public async Task<(int Sucessos, string Mensagem)> SincronizarListaArrematesAsync(string sheetUrl, string sheetName, List<Arremate> arremates)
        {
            var spreadsheetId = ExtrairSpreadsheetId(sheetUrl);
            if (string.IsNullOrEmpty(spreadsheetId))
            {
                return (0, "URL da planilha Google Sheets inválida.");
            }

            if (arremates == null || arremates.Count == 0)
            {
                return (0, "Nenhum arremate encontrado para sincronizar.");
            }

            try
            {
                var service = ObterSheetsService();
                var sheet = string.IsNullOrWhiteSpace(sheetName) ? "vendas" : sheetName;

                await GarantirCabecalhoAsync(service, spreadsheetId, sheet);

                var rows = new List<IList<object>>();
                foreach (var a in arremates)
                {
                    var row = new List<object>
                    {
                        a.CodigoLive?.ToString() ?? "",
                        "", // Intern
                        a.DescricaoManual ?? "",
                        a.ValorArremate?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) ?? "0.00",
                        a.Arrematante ?? ""
                    };

                    if (!string.IsNullOrWhiteSpace(a.Fila))
                    {
                        var filas = a.Fila.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var f in filas)
                        {
                            row.Add(f.Trim());
                        }
                    }

                    rows.Add(row);
                }

                var valueRange = new ValueRange { Values = rows };
                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, $"{sheet}!A:Z");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                var result = await appendRequest.ExecuteAsync();
                _logger.LogInformation("Sincronização em lote: {Count} arremates gravados no Google Sheets: {Updates}", rows.Count, result.Updates?.UpdatedRange);

                return (rows.Count, $"{rows.Count} arremates sincronizados com a planilha Google com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na sincronização em lote com Google Sheets: {Erro}", ex.Message);
                return (0, $"Erro ao sincronizar com Google Sheets: {ex.Message}");
            }
        }
    }
}
