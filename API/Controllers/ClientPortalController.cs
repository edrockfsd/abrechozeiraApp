using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using System.Net.Http;
using System.IO;

namespace ABrechozeiraApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientPortalController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;
        private readonly ILogger<ClientPortalController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ClientPortalController(
            AbrechozeiraContext context,
            ILogger<ClientPortalController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("lives")]
        public async Task<IActionResult> GetLives()
        {
            var lives = await _context.Live
                .OrderByDescending(l => l.DataLive)
                .Select(l => new
                {
                    l.Id,
                    l.Titulo,
                    l.DataLive,
                    HasSheet = !string.IsNullOrWhiteSpace(l.GoogleSheetUrl)
                })
                .ToListAsync();

            return Ok(lives);
        }

        [HttpGet("meus-arremates")]
        public async Task<IActionResult> GetMeusArremates([FromQuery] int liveId, [FromQuery] string nickInstagram)
        {
            if (string.IsNullOrWhiteSpace(nickInstagram))
                return BadRequest(new { message = "Nick do Instagram é obrigatório." });

            var nickFormatado = nickInstagram.Trim().ToLower().Replace("@", "");

            var arremates = await _context.Arremate
                .Where(a => a.LiveId == liveId && a.Arrematante.ToLower().Replace("@", "") == nickFormatado)
                .Select(a => new
                {
                    a.Id,
                    a.Arrematante,
                    a.ValorArremate,
                    a.DescricaoManual,
                    Produto = a.Produto != null ? a.Produto.Descricao : null,
                    a.CodigoLive
                })
                .ToListAsync();

            return Ok(new
            {
                TotalItens = arremates.Count,
                TotalValor = arremates.Sum(a => a.ValorArremate ?? 0),
                Arremates = arremates
            });
        }

        [HttpPost("lives/{liveId}/sync-sheet")]
        public async Task<IActionResult> SyncSheet(int liveId)
        {
            var live = await _context.Live.FindAsync(liveId);
            if (live == null) return NotFound(new { message = "Live não encontrada." });
            
            if (string.IsNullOrWhiteSpace(live.GoogleSheetUrl))
                return BadRequest(new { message = "Esta Live não possui uma URL de planilha configurada." });

            try
            {
                var downloadUrl = live.GoogleSheetUrl;
                
                // Se for um link do Google Sheets que não seja o export direto, vamos converter
                if (downloadUrl.Contains("docs.google.com/spreadsheets") && !downloadUrl.Contains("/export?format=csv"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(downloadUrl, @"/d/([a-zA-Z0-9-_]+)");
                    var gidMatch = System.Text.RegularExpressions.Regex.Match(downloadUrl, @"gid=([0-9]+)");
                    if (match.Success)
                    {
                        var id = match.Groups[1].Value;
                        var gid = gidMatch.Success ? gidMatch.Groups[1].Value : "0";
                        downloadUrl = $"https://docs.google.com/spreadsheets/d/{id}/export?format=csv&gid={gid}";
                    }
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                var response = await client.GetAsync(downloadUrl);
                
                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { message = "Não foi possível baixar o CSV da URL fornecida." });

                var csvContent = await response.Content.ReadAsStringAsync();
                
                var linhas = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var arrematesPlanilha = new List<Arremate>();
                
                for (int i = 1; i < linhas.Length; i++)
                {
                    var linha = linhas[i];
                    if (string.IsNullOrWhiteSpace(linha)) continue;

                    var cols = ParseCsvLine(linha);
                    if (cols.Count < 5) continue;

                    var externoStr = cols[0];
                    var descricao = cols[2];
                    var valorStr = cols[3];
                    var comprador = cols[4];

                    if (string.IsNullOrWhiteSpace(comprador)) continue;

                    var filaNomes = new List<string>();
                    for (int j = 5; j < cols.Count; j++)
                    {
                        if (!string.IsNullOrWhiteSpace(cols[j]))
                            filaNomes.Add(cols[j].Trim());
                    }
                    var filaConcat = string.Join("; ", filaNomes);

                    valorStr = valorStr.Replace("R$", "").Trim();
                    if (valorStr.Contains(".") && valorStr.Contains(","))
                    {
                        if (valorStr.LastIndexOf('.') > valorStr.LastIndexOf(','))
                            valorStr = valorStr.Replace(",", "");
                        else
                            valorStr = valorStr.Replace(".", "").Replace(",", ".");
                    }
                    else if (valorStr.Contains(","))
                    {
                        valorStr = valorStr.Replace(",", ".");
                    }

                    decimal.TryParse(valorStr, 
                        System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        out var valorArremate);

                    int.TryParse(externoStr, out var codigoLive);

                    arrematesPlanilha.Add(new Arremate
                    {
                        LiveId = liveId,
                        CodigoLive = codigoLive,
                        Arrematante = comprador.Trim(),
                        ValorArremate = valorArremate,
                        DescricaoManual = descricao.Trim(),
                        Fila = filaConcat,
                        ImportadoPlanilha = true,
                        DataArremate = DateTime.Now,
                        DataAlteracao = DateTime.Now
                    });
                }

                // 1. Deletar apenas arremates antigos importados da planilha para esta live
                var antigos = _context.Arremate.Where(a => a.LiveId == liveId && a.ImportadoPlanilha);
                _context.Arremate.RemoveRange(antigos);
                
                // 2. Adicionar novos
                _context.Arremate.AddRange(arrematesPlanilha);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Sincronização concluída com sucesso. {arrematesPlanilha.Count} arremates importados." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar planilha para a live {LiveId}", liveId);
                return StatusCode(500, new { message = "Erro interno ao processar a planilha." });
            }
        }

        private List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new System.Text.StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            fields.Add(currentField.ToString());
            return fields;
        }
    }
}
