using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ABrechozeiraApp.Controllers;
using ABrechozeiraApp.DTOs;
using ABrechozeiraApp.Hubs;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Services
{
    public class LiveTrackingState
    {
        public int LiveId { get; set; }
        public long? LiveVideoId { get; set; }
        public bool IsTracking { get; set; }
        public string? ActiveCode { get; set; }
        public string? ActiveDescription { get; set; }
        public decimal? ActiveValue { get; set; }
        public List<MatchDto> Matches { get; set; } = new();
        public HashSet<string> SeenUsers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<ComentarioLiveDto> MessageBuffer { get; set; } = new();
        public object LockObject { get; } = new();
    }

    public class LiveTrackerService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<LiveHub> _hubContext;
        private readonly ILogger<LiveTrackerService> _logger;
        private readonly ConcurrentDictionary<int, LiveTrackingState> _lives = new();
        private LiveAtivaInfo? _liveAtiva;
        private readonly object _liveAtivaLock = new();

        public LiveTrackerService(
            IServiceScopeFactory scopeFactory,
            IHubContext<LiveHub> hubContext,
            ILogger<LiveTrackerService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        public void RegistrarLiveAtiva(long liveVideoId, int liveId, string titulo)
        {
            lock (_liveAtivaLock)
            {
                _liveAtiva = new LiveAtivaInfo
                {
                    IsLive = true,
                    LiveVideoId = liveVideoId,
                    LiveId = liveId,
                    Titulo = titulo
                };
            }
        }

        public void LimparLiveAtiva()
        {
            lock (_liveAtivaLock)
            {
                _liveAtiva = null;
            }
        }

        public LiveAtivaInfo ObterLiveAtiva()
        {
            lock (_liveAtivaLock)
            {
                if (_liveAtiva == null || !_liveAtiva.IsLive)
                {
                    return new LiveAtivaInfo { IsLive = false };
                }

                if (_liveAtiva.LiveId.HasValue && _lives.TryGetValue(_liveAtiva.LiveId.Value, out var state))
                {
                    lock (state.LockObject)
                    {
                        _liveAtiva.TotalComentarios = state.MessageBuffer.Count;
                    }
                }

                return _liveAtiva;
            }
        }

        private LiveTrackingState ObterOuCriarEstado(int liveId, long? liveVideoId = null)
        {
            return _lives.GetOrAdd(liveId, id => new LiveTrackingState
            {
                LiveId = id,
                LiveVideoId = liveVideoId
            });
        }

        public void VincularLiveVideoId(int liveId, long liveVideoId)
        {
            var state = ObterOuCriarEstado(liveId, liveVideoId);
            lock (state.LockObject)
            {
                state.LiveVideoId = liveVideoId;
            }
        }

        public long? ObterLiveVideoId(int liveId)
        {
            if (_lives.TryGetValue(liveId, out var state))
            {
                lock (state.LockObject)
                {
                    if (state.LiveVideoId.HasValue && state.LiveVideoId.Value > 0)
                    {
                        return state.LiveVideoId.Value;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Inicia o rastreio de um novo código de peça, realizando a varredura retroativa
        /// nos comentários dos últimos minutos em ordem estrita por CommentTimestamp.
        /// </summary>
        public async Task<EstadoRastreioDto> IniciarRastreioAsync(IniciarRastreioRequest request)
        {
            var state = ObterOuCriarEstado(request.LiveId, request.LiveVideoId);

            lock (state.LockObject)
            {
                state.IsTracking = true;
                state.ActiveCode = request.Codigo.Trim();
                state.ActiveDescription = request.Descricao.Trim();
                state.ActiveValue = request.Valor;
                state.Matches.Clear();
                state.SeenUsers.Clear();
                if (request.LiveVideoId.HasValue)
                {
                    state.LiveVideoId = request.LiveVideoId.Value;
                }
            }

            _logger.LogInformation("Iniciando rastreio da peça '{Codigo}' na Live {LiveId} (Valor: {Valor})",
                state.ActiveCode, request.LiveId, state.ActiveValue);

            // 1. Varredura retroativa nos últimos 5 minutos no banco de dados e buffer
            var retroMatches = await ExecutarVarreduraRetroativaAsync(state);

            lock (state.LockObject)
            {
                foreach (var match in retroMatches)
                {
                    if (state.SeenUsers.Add(match.Username))
                    {
                        match.Posicao = state.Matches.Count + 1;
                        state.Matches.Add(match);
                    }
                }
            }

            var estadoDto = ObterEstadoAtual(request.LiveId);

            // Notifica todos os clientes conectados na sala da live via SignalR
            await _hubContext.Clients.Group($"Live_{request.LiveId}")
                .SendAsync("StatusRastreio", estadoDto);

            return estadoDto;
        }

        /// <summary>
        /// Para o rastreio da peça atual
        /// </summary>
        public async Task<EstadoRastreioDto> PararRastreioAsync(int liveId)
        {
            var state = ObterOuCriarEstado(liveId);
            lock (state.LockObject)
            {
                state.IsTracking = false;
                state.ActiveCode = null;
                state.ActiveDescription = null;
                state.ActiveValue = null;
                state.Matches.Clear();
                state.SeenUsers.Clear();
            }

            var estadoDto = ObterEstadoAtual(liveId);
            await _hubContext.Clients.Group($"Live_{liveId}")
                .SendAsync("StatusRastreio", estadoDto);

            return estadoDto;
        }

        /// <summary>
        /// Remove um participante específico (comprador ou fila) e recalcula as posições
        /// </summary>
        public async Task<EstadoRastreioDto> RemoverMatchAsync(int liveId, int index)
        {
            var state = ObterOuCriarEstado(liveId);
            lock (state.LockObject)
            {
                if (index >= 0 && index < state.Matches.Count)
                {
                    var removido = state.Matches[index];
                    state.Matches.RemoveAt(index);
                    state.SeenUsers.Remove(removido.Username);

                    // Recalcula posições
                    for (int i = 0; i < state.Matches.Count; i++)
                    {
                        state.Matches[i].Posicao = i + 1;
                    }

                    _logger.LogInformation("Usuário @{Username} removido da posição {Posicao} da peça {Codigo}",
                        removido.Username, index + 1, state.ActiveCode);
                }
            }

            var estadoDto = ObterEstadoAtual(liveId);
            await _hubContext.Clients.Group($"Live_{liveId}")
                .SendAsync("MatchesAtualizados", estadoDto.Matches);

            return estadoDto;
        }

        /// <summary>
        /// Retorna o estado atual da live para novo carregamento da tela (F5)
        /// </summary>
        public EstadoRastreioDto ObterEstadoAtual(int liveId)
        {
            var state = ObterOuCriarEstado(liveId);
            lock (state.LockObject)
            {
                return new EstadoRastreioDto
                {
                    LiveId = liveId,
                    IsTracking = state.IsTracking,
                    ActiveCode = state.ActiveCode,
                    ActiveDescription = state.ActiveDescription,
                    ActiveValue = state.ActiveValue,
                    Matches = state.Matches.ToList()
                };
            }
        }

        /// <summary>
        /// Ponto de entrada global usado pelo InstagramLivePollingService.
        /// Despacha para a sessão correspondente ao liveVideoId ou para a live ativa mais recente.
        /// </summary>
        public async Task ProcessarNovoComentarioGlobalAsync(long liveVideoId, ComentarioLiveDto comentario)
        {
            // Encontra sessões que correspondem ao LiveVideoId ou estão ativas
            var sessoesAlvo = _lives.Values
                .Where(s => s.LiveVideoId == liveVideoId || (s.LiveVideoId == null && s.IsTracking))
                .ToList();

            if (sessoesAlvo.Count == 0)
            {
                // Se o operador ainda não abriu a tela mas a live está rolando, busca o último LiveId do banco
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AbrechozeiraContext>();
                    var vidStr = liveVideoId.ToString();
                    var ultimaLive = await db.Live
                        .Where(l => l.Observacoes != null && l.Observacoes.Contains(vidStr))
                        .FirstOrDefaultAsync();

                    if (ultimaLive == null)
                    {
                        ultimaLive = await db.Live.OrderByDescending(l => l.Id).FirstOrDefaultAsync();
                    }

                    if (ultimaLive != null)
                    {
                        var estado = ObterOuCriarEstado(ultimaLive.Id, liveVideoId);
                        sessoesAlvo.Add(estado);
                    }
                }
                catch {}
            }

            foreach (var sessao in sessoesAlvo)
            {
                if (sessao.LiveVideoId == null)
                {
                    sessao.LiveVideoId = liveVideoId;
                }
                await ProcessarNovoComentarioAsync(sessao.LiveId, comentario);
            }

            // Notifica pelo grupo do próprio LiveVideoId apenas se nenhuma sessão de LiveId foi notificada
            if (sessoesAlvo.Count == 0)
            {
                await _hubContext.Clients.Group($"Live_{liveVideoId}")
                    .SendAsync("NovoComentario", comentario);
            }
        }

        /// <summary>
        /// Processa um novo comentário recebido (do InstagramLivePollingService ou SSN),
        /// adiciona ao buffer e verifica match com a peça ativa se estiver rastreando.
        /// </summary>
        public async Task ProcessarNovoComentarioAsync(int liveId, ComentarioLiveDto comentario)
        {
            var state = ObterOuCriarEstado(liveId);

            lock (state.LockObject)
            {
                // Manter buffer das últimas 500 mensagens
                state.MessageBuffer.Add(comentario);
                if (state.MessageBuffer.Count > 500)
                {
                    state.MessageBuffer.RemoveAt(0);
                }
            }

            // Enviar mensagem de chat via SignalR para o monitor de chat da tela
            await _hubContext.Clients.Group($"Live_{liveId}")
                .SendAsync("NovoComentario", comentario);

            // Checar se há código sendo rastreado
            MatchDto? novoMatch = null;
            lock (state.LockObject)
            {
                if (state.IsTracking && !string.IsNullOrWhiteSpace(state.ActiveCode))
                {
                    if (IsMatch(comentario.CommentText, state.ActiveCode))
                    {
                        if (state.SeenUsers.Add(comentario.Username))
                        {
                            novoMatch = new MatchDto
                            {
                                Username = comentario.Username,
                                CommentText = comentario.CommentText,
                                CommentTimestamp = comentario.CreatedAt,
                                Posicao = state.Matches.Count + 1,
                                InstagramCommentId = comentario.Id.ToString(),
                                IsRetroativo = false
                            };

                            state.Matches.Add(novoMatch);
                            _logger.LogInformation("🎯 MATCH DETECTADO! [{Posicao}] @{Username}: '{Texto}' às {TimestampFmt}",
                                novoMatch.Posicao, novoMatch.Username, novoMatch.CommentText, novoMatch.CommentTimestampFmt);
                        }
                    }
                }
            }

            if (novoMatch != null)
            {
                await _hubContext.Clients.Group($"Live_{liveId}")
                    .SendAsync("MatchDetectado", novoMatch);
            }
        }

        /// <summary>
        /// Varre o banco de dados e o buffer em busca de quem já digitou o código nos últimos 5 minutos,
        /// ordenando estritamente por CreatedAt ascendente com precisão de milissegundos.
        /// ISOLAMENTO ESTRITO: nunca pesquisa comentários de outras lives!
        /// </summary>
        private async Task<List<MatchDto>> ExecutarVarreduraRetroativaAsync(LiveTrackingState state)
        {
            var codigo = state.ActiveCode;
            if (string.IsNullOrWhiteSpace(codigo)) return new();

            var matches = new List<MatchDto>();
            var limiteTempo = DateTime.Now.AddMinutes(-10);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AbrechozeiraContext>();

                // Garante que o LiveVideoId está resolvido para esta live específica
                if (!state.LiveVideoId.HasValue || state.LiveVideoId.Value <= 0)
                {
                    var live = await db.Live.FindAsync(state.LiveId);
                    if (live?.Observacoes != null)
                    {
                        var m = Regex.Match(live.Observacoes, @"ID:\s*(\d+)");
                        if (m.Success && long.TryParse(m.Groups[1].Value, out var parsedVid))
                        {
                            state.LiveVideoId = parsedVid;
                        }
                    }
                }

                // OBRIGATÓRIO: A varredura retroativa DEVE filtrar estritamente pela live atual
                if (!state.LiveVideoId.HasValue || state.LiveVideoId.Value <= 0)
                {
                    _logger.LogWarning("Varredura retroativa ignorada: LiveVideoId não identificado para Live {LiveId}", state.LiveId);
                    return matches;
                }

                // Buscar comentários recentes exclusivamente desta live ordenados por CreatedAt
                var comentarios = await db.ComentarioLive.AsNoTracking()
                    .Where(c => c.LiveSessionId == state.LiveVideoId.Value && c.CreatedAt >= limiteTempo)
                    .OrderBy(c => c.CreatedAt)
                    .ThenBy(c => c.Id)
                    .ToListAsync();

                var seenInRetro = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var c in comentarios)
                {
                    if (IsMatch(c.CommentText, codigo))
                    {
                        if (seenInRetro.Add(c.Username))
                        {
                            matches.Add(new MatchDto
                            {
                                Username = c.Username,
                                CommentText = c.CommentText,
                                CommentTimestamp = c.CreatedAt,
                                InstagramCommentId = c.InstagramCommentId ?? c.Id.ToString(),
                                IsRetroativo = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro na varredura retroativa no banco: {Erro}", ex.Message);
            }

            return matches;
        }

        /// <summary>
        /// Regra de match: match estrito e idêntico do código digitado (apenas trim nas pontas e case-insensitive).
        /// "055" NÃO bate com "55", "001" NÃO bate com "1" ou "01", "flor" NÃO bate com "fl0r".
        /// </summary>
        private static bool IsMatch(string? texto, string? codigo)
        {
            if (string.IsNullOrWhiteSpace(texto) || string.IsNullOrWhiteSpace(codigo))
                return false;

            var t = texto.Trim();
            var c = codigo.Trim();

            // Match estrito de texto (não faz equivalência numérica como 055 == 55)
            return string.Equals(t, c, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Auditoria completa: busca todos os comentários que acertaram o código na live inteira,
        /// ordenados estritamente por CreatedAt ascendente, com texto formatado para envio a clientes.
        /// ISOLAMENTO ESTRITO: nunca pesquisa comentários de outras lives!
        /// </summary>
        public async Task<List<AuditoriaMatchDto>> ObterAuditoriaAsync(int liveId, string codigo, long? liveVideoId = null)
        {
            var matches = new List<AuditoriaMatchDto>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AbrechozeiraContext>();

                // Garante a resolução autoritativa do LiveVideoId para esta Live específica
                long? videoIdAutoritativo = null;

                if (_lives.TryGetValue(liveId, out var st) && st.LiveVideoId.HasValue && st.LiveVideoId.Value > 0)
                {
                    videoIdAutoritativo = st.LiveVideoId;
                }
                else
                {
                    var live = await db.Live.FindAsync(liveId);
                    if (live?.Observacoes != null)
                    {
                        var m = Regex.Match(live.Observacoes, @"ID:\s*(\d+)");
                        if (m.Success && long.TryParse(m.Groups[1].Value, out var parsedVid))
                        {
                            videoIdAutoritativo = parsedVid;
                            VincularLiveVideoId(liveId, parsedVid);
                        }
                    }

                    if (!videoIdAutoritativo.HasValue && live != null)
                    {
                        // Tenta achar por data da Live
                        var dataInicio = live.DataLive.Date;
                        var dataFim = dataInicio.AddDays(1);
                        var comRecente = await db.ComentarioLive
                            .Where(c => c.CreatedAt >= dataInicio && c.CreatedAt < dataFim && c.LiveSessionId != null)
                            .OrderByDescending(c => c.Id)
                            .FirstOrDefaultAsync();

                        if (comRecente != null)
                        {
                            videoIdAutoritativo = comRecente.LiveSessionId;
                            VincularLiveVideoId(liveId, comRecente.LiveSessionId.Value);
                        }
                    }
                }

                // Se não achou na liveId, usa o liveVideoId passado por parâmetro
                var vidFinal = videoIdAutoritativo ?? liveVideoId;

                // OBRIGATÓRIO: Se ainda assim não encontrar, nunca buscar no banco sem filtro (evita vazar outras lives!)
                if (!vidFinal.HasValue || vidFinal.Value <= 0)
                {
                    _logger.LogWarning("Auditoria cancelada: LiveVideoId não identificado para Live {LiveId}", liveId);
                    return matches;
                }

                var comentarios = await db.ComentarioLive.AsNoTracking()
                    .Where(c => c.LiveSessionId == vidFinal.Value)
                    .OrderBy(c => c.CreatedAt)
                    .ThenBy(c => c.Id)
                    .ToListAsync();

                int pos = 1;
                foreach (var c in comentarios)
                {
                    if (IsMatch(c.CommentText, codigo) && seen.Add(c.Username))
                    {
                        matches.Add(new AuditoriaMatchDto
                        {
                            Posicao = pos,
                            Username = c.Username,
                            TextoDigitado = c.CommentText,
                            Timestamp = c.CreatedAt,
                            Status = pos == 1 ? "🏆 Comprador" : $"📋 Fila {pos - 1}"
                        });
                        pos++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao consultar auditoria no banco: {Erro}", ex.Message);
            }

            // Se ainda houver matches na memória da sessão atual que não foram persistidos
            if (_lives.TryGetValue(liveId, out var state))
            {
                lock (state.LockObject)
                {
                    if (string.Equals(state.ActiveCode, codigo, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var m in state.Matches)
                        {
                            if (seen.Add(m.Username))
                            {
                                int pos = matches.Count + 1;
                                matches.Add(new AuditoriaMatchDto
                                {
                                    Posicao = pos,
                                    Username = m.Username,
                                    TextoDigitado = m.CommentText,
                                    Timestamp = m.CommentTimestamp,
                                    Status = pos == 1 ? "🏆 Comprador" : $"📋 Fila {pos - 1}"
                                });
                            }
                        }
                    }
                }
            }

            return matches.OrderBy(m => m.Timestamp).ToList();
        }
    }
}
