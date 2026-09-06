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

namespace ABrechozeiraApp.Services
{
    public class GoogleSheetsSyncService
    {
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

        private SheetsService ObterSheetsService(string credPath)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
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
        /// Garante que a planilha possui o cabeçalho completo incluindo as colunas de auditoria de data/hora
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

            var credPath = LocalizarArquivoCredenciais();
            if (credPath == null)
            {
                _logger.LogWarning("Arquivo de credenciais do Google Sheets (service-account.json) não encontrado.");
                return false;
            }

            try
            {
                var service = ObterSheetsService(credPath);
                var sheetName = string.IsNullOrWhiteSpace(request.SheetName) ? "vendas" : request.SheetName;

                await GarantirCabecalhoAsync(service, spreadsheetId, sheetName);

                // Montar a linha exatamente como a planilha do usuário espera:
                // [Externo, Intern, Descricao, Valor, Comprador, Fila 1, Fila 2, Fila 3, ...]
                // Sem horários nas colunas de fila: apenas os nomes de usuário!
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
    }
}
