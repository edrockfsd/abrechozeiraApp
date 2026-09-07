using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ABrechozeiraApp.DTOs;
using ABrechozeiraApp.Models;
using ABrechozeiraApp.Services;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveTrackerController : ControllerBase
    {
        private readonly LiveTrackerService _trackerService;
        private readonly GoogleSheetsSyncService _sheetsSync;
        private readonly AbrechozeiraContext _context;
        private readonly ILogger<LiveTrackerController> _logger;

        public LiveTrackerController(
            LiveTrackerService trackerService,
            GoogleSheetsSyncService sheetsSync,
            AbrechozeiraContext context,
            ILogger<LiveTrackerController> logger)
        {
            _trackerService = trackerService;
            _sheetsSync = sheetsSync;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Inicia o rastreio de um novo código de peça
        /// </summary>
        [HttpPost("iniciar-rastreio")]
        public async Task<IActionResult> IniciarRastreio([FromBody] IniciarRastreioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Codigo))
            {
                return BadRequest(new { erro = "O código da peça é obrigatório." });
            }

            var estado = await _trackerService.IniciarRastreioAsync(request);
            return Ok(estado);
        }

        /// <summary>
        /// Para o rastreio da peça ativa
        /// </summary>
        [HttpPost("parar-rastreio/{liveId}")]
        public async Task<IActionResult> PararRastreio(int liveId)
        {
            var estado = await _trackerService.PararRastreioAsync(liveId);
            return Ok(estado);
        }

        /// <summary>
        /// Remove um participante específico (comprador ou fila) da peça ativa
        /// </summary>
        [HttpDelete("remover-match/{liveId}/{index}")]
        public async Task<IActionResult> RemoverMatch(int liveId, int index)
        {
            var estado = await _trackerService.RemoverMatchAsync(liveId, index);
            return Ok(estado);
        }

        /// <summary>
        /// Informa se há uma transmissão ao vivo ativa sendo capturada no Instagram agora
        /// </summary>
        [HttpGet("live-ativa")]
        public IActionResult GetLiveAtiva()
        {
            var info = _trackerService.ObterLiveAtiva();
            return Ok(info);
        }

        /// <summary>
        /// Obtém informações consolidadas da live (LiveVideoId exato, total de comentários, status)
        /// </summary>
        [HttpGet("info/{liveId}")]
        public async Task<IActionResult> ObterInfoLive(int liveId)
        {
            var live = await _context.Live.FindAsync(liveId);
            if (live == null) return NotFound("Live não encontrada.");

            long? liveVideoId = null;

            // 1. Tentar obter da live ativa em memória
            var liveAtiva = _trackerService.ObterLiveAtiva();
            if (liveAtiva.IsLive && liveAtiva.LiveId == liveId && liveAtiva.LiveVideoId.HasValue)
            {
                liveVideoId = liveAtiva.LiveVideoId.Value;
            }

            // 2. Tentar obter do estado em memória
            if (!liveVideoId.HasValue)
            {
                liveVideoId = _trackerService.ObterLiveVideoId(liveId);
            }

            // 3. Extrair de Observações da Live: "Live Instagram detectada automaticamente (ID: 17966501787169421)"
            if (!liveVideoId.HasValue && !string.IsNullOrWhiteSpace(live.Observacoes))
            {
                var match = Regex.Match(live.Observacoes, @"ID:\s*(\d+)");
                if (match.Success && long.TryParse(match.Groups[1].Value, out var parsedId))
                {
                    liveVideoId = parsedId;
                }
            }

            // 4. Se ainda não achou, buscar na tabela ComentarioLive pelo dia da Live
            if (!liveVideoId.HasValue)
            {
                var dataInicio = live.DataLive.Date;
                var dataFim = dataInicio.AddDays(1);
                var comentarioMaisRecente = await _context.ComentarioLive
                    .Where(c => c.CreatedAt >= dataInicio && c.CreatedAt < dataFim && c.LiveSessionId != null)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                if (comentarioMaisRecente != null)
                {
                    liveVideoId = comentarioMaisRecente.LiveSessionId;
                }
            }

            // 5. Vincular no estado em memória para garantir consistência em todas as operações
            if (liveVideoId.HasValue)
            {
                _trackerService.VincularLiveVideoId(liveId, liveVideoId.Value);
            }

            // Contagem de comentários exclusivos desta live
            int totalComentarios = 0;
            if (liveVideoId.HasValue)
            {
                totalComentarios = await _context.ComentarioLive
                    .CountAsync(c => c.LiveSessionId == liveVideoId.Value);
            }

            return Ok(new
            {
                liveId = live.Id,
                titulo = live.Titulo,
                dataLive = live.DataLive,
                googleSheetUrl = live.GoogleSheetUrl,
                liveVideoId = liveVideoId,
                totalComentarios = totalComentarios,
                isLive = liveAtiva.IsLive && liveAtiva.LiveId == liveId
            });
        }

        /// <summary>
        /// Obtém o estado atual da live (recuperação de tela/F5)
        /// </summary>
        [HttpGet("estado-atual/{liveId}")]
        public IActionResult GetEstadoAtual(int liveId)
        {
            var estado = _trackerService.ObterEstadoAtual(liveId);
            return Ok(estado);
        }

        /// <summary>
        /// Confirma o arremate da peça: grava na tabela Arremate E sincroniza no Google Sheets em tempo real
        /// </summary>
        [HttpPost("confirmar-arremate")]
        public async Task<IActionResult> ConfirmarArremate([FromBody] ConfirmarArremateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Codigo))
                {
                    return BadRequest(new { erro = "Código da peça não pode ser vazio." });
                }

                // 1. Extrair comprador e lista da fila
                var comprador = request.Arrematante;
                var compradorMatch = request.Matches.FirstOrDefault(m => m.Posicao == 1);
                if (string.IsNullOrWhiteSpace(comprador) && compradorMatch != null)
                {
                    comprador = compradorMatch.Username;
                }

                var filaUsers = request.Matches
                    .Where(m => m.Posicao > 1)
                    .OrderBy(m => m.Posicao)
                    .Select(m => m.Username)
                    .ToList();

                var filaStr = filaUsers.Any() ? string.Join(", ", filaUsers) : null;

                int? codigoLiveInt = null;
                if (int.TryParse(request.Codigo, out var parsedCod))
                {
                    codigoLiveInt = parsedCod;
                }

                // 2. Persistir na tabela Arremate
                var arremate = new Arremate
                {
                    LiveId = request.LiveId,
                    CodigoLive = codigoLiveInt,
                    Arrematante = comprador ?? "Sem Comprador",
                    ValorArremate = request.Valor,
                    DescricaoManual = request.Descricao,
                    Fila = filaStr,
                    ImportadoPlanilha = false,
                    DataArremate = compradorMatch?.CommentTimestamp ?? DateTime.Now,
                    DataAlteracao = DateTime.Now,
                    Observacoes = compradorMatch != null
                        ? $"Match exato no chat às {compradorMatch.CommentTimestampFmt}: \"{compradorMatch.CommentText}\""
                        : null
                };

                _context.Arremate.Add(arremate);
                await _context.SaveChangesAsync();

                // 3. Sincronizar em tempo real no Google Sheets (se configurado na Live ou na request)
                string? sheetUrl = request.GoogleSheetUrl;
                var live = await _context.Live.FindAsync(request.LiveId);
                if (string.IsNullOrWhiteSpace(sheetUrl))
                {
                    sheetUrl = live?.GoogleSheetUrl;
                }

                if (string.IsNullOrWhiteSpace(sheetUrl))
                {
                    var ultimaComPlanilha = await _context.Live
                        .Where(l => !string.IsNullOrEmpty(l.GoogleSheetUrl))
                        .OrderByDescending(l => l.Id)
                        .FirstOrDefaultAsync();

                    sheetUrl = ultimaComPlanilha?.GoogleSheetUrl ?? "https://docs.google.com/spreadsheets/d/1HUEcIGWlgdcMuBi1zIhYX4sm660UT_ttyUkb_XhAS3o/edit?gid=1053114646#gid=1053114646";

                    if (live != null && string.IsNullOrEmpty(live.GoogleSheetUrl))
                    {
                        live.GoogleSheetUrl = sheetUrl;
                        await _context.SaveChangesAsync();
                    }
                }

                bool sheetSincronizado = false;
                if (!string.IsNullOrWhiteSpace(sheetUrl))
                {
                    request.GoogleSheetUrl = sheetUrl;
                    sheetSincronizado = await _sheetsSync.SincronizarPecaNaPlanilhaAsync(request);
                }

                // 4. Limpar o rastreio da peça ativa para liberar para a próxima
                await _trackerService.PararRastreioAsync(request.LiveId);

                return Ok(new
                {
                    sucesso = true,
                    arremateId = arremate.Id,
                    codigo = request.Codigo,
                    comprador = arremate.Arrematante,
                    valor = arremate.ValorArremate,
                    fila = arremate.Fila,
                    googleSheetSincronizado = sheetSincronizado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao confirmar arremate da peça {Codigo}", request.Codigo);
                return StatusCode(500, new { erro = $"Erro ao confirmar arremate: {ex.Message}" });
            }
        }

        /// <summary>
        /// Auditoria incontestável de quem digitou o código na live inteira (ordenado por milissegundos)
        /// </summary>
        [HttpGet("auditoria/{liveId}/{codigo}")]
        public async Task<IActionResult> ObterAuditoria(int liveId, string codigo, [FromQuery] long? liveVideoId = null)
        {
            var auditoria = await _trackerService.ObterAuditoriaAsync(liveId, codigo, liveVideoId);
            return Ok(auditoria);
        }

        /// <summary>
        /// Endpoint de contingência: permite injetar mensagem do Social Stream Ninja (se o operador ativar backup)
        /// </summary>
        [HttpPost("injetar-comentario-ssn")]
        public async Task<IActionResult> InjetarComentarioSSN([FromBody] SsnMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ChatName) || string.IsNullOrWhiteSpace(request.ChatMessage))
            {
                return BadRequest(new { erro = "ChatName e ChatMessage são obrigatórios." });
            }

            var timestamp = request.TimestampMs.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(request.TimestampMs.Value).ToLocalTime().DateTime
                : DateTime.Now;

            var novoComentario = new ComentarioLive
            {
                Username = request.ChatName,
                CommentText = request.ChatMessage,
                CommentTimestamp = timestamp,
                CreatedAt = DateTime.Now,
                LiveSessionId = request.LiveId
            };
            _context.ComentarioLive.Add(novoComentario);
            await _context.SaveChangesAsync();

            var dto = new ComentarioLiveDto
            {
                Id = novoComentario.Id,
                Username = request.ChatName,
                CommentText = request.ChatMessage,
                CommentTimestamp = timestamp,
                CreatedAt = DateTime.Now
            };

            await _trackerService.ProcessarNovoComentarioAsync(request.LiveId, dto);
            return Ok(new { sucesso = true });
        }

        /// <summary>
        /// Sincroniza em lote todos os arremates já gravados no banco para a planilha Google
        /// </summary>
        [HttpPost("sincronizar-planilha/{liveId}")]
        public async Task<IActionResult> SincronizarPlanilhaRetroativo(int liveId, [FromBody] SincronizarPlanilhaRequest? body)
        {
            var live = await _context.Live.FindAsync(liveId);
            if (live == null) return NotFound("Live não encontrada.");

            var sheetUrl = body?.GoogleSheetUrl ?? live.GoogleSheetUrl;
            if (string.IsNullOrWhiteSpace(sheetUrl))
            {
                var ultimaComPlanilha = await _context.Live
                    .Where(l => !string.IsNullOrEmpty(l.GoogleSheetUrl))
                    .OrderByDescending(l => l.Id)
                    .FirstOrDefaultAsync();

                sheetUrl = ultimaComPlanilha?.GoogleSheetUrl ?? "https://docs.google.com/spreadsheets/d/1HUEcIGWlgdcMuBi1zIhYX4sm660UT_ttyUkb_XhAS3o/edit?gid=1053114646#gid=1053114646";
            }

            if (string.IsNullOrWhiteSpace(live.GoogleSheetUrl) || live.GoogleSheetUrl != sheetUrl)
            {
                live.GoogleSheetUrl = sheetUrl;
                await _context.SaveChangesAsync();
            }

            var arremates = await _context.Arremate
                .Where(a => a.LiveId == liveId)
                .OrderBy(a => a.Id)
                .ToListAsync();

            var sheetName = body?.SheetName ?? "vendas";
            var (sucessos, msg) = await _sheetsSync.SincronizarListaArrematesAsync(sheetUrl, sheetName, arremates);

            return Ok(new
            {
                sucesso = sucessos > 0,
                total = sucessos,
                mensagem = msg,
                googleSheetUrl = sheetUrl
            });
        }

        /// <summary>
        /// Salva a URL da planilha Google diretamente no registro da Live
        /// </summary>
        [HttpPost("configurar-planilha/{liveId}")]
        public async Task<IActionResult> ConfigurarPlanilha(int liveId, [FromBody] ConfigurarPlanilhaRequest request)
        {
            var live = await _context.Live.FindAsync(liveId);
            if (live == null) return NotFound("Live não encontrada.");

            live.GoogleSheetUrl = request.GoogleSheetUrl?.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { sucesso = true, googleSheetUrl = live.GoogleSheetUrl });
        }
    }
}

