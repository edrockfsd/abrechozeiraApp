using System;
using System.Collections.Generic;

namespace ABrechozeiraApp.DTOs
{
    public class MatchDto
    {
        public string Username { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentTimestamp { get; set; }
        /// <summary>
        /// Carimbo de data/hora com precisão de milissegundos para auditoria incontestável (HH:mm:ss.fff)
        /// </summary>
        public string CommentTimestampFmt => CommentTimestamp.ToString("HH:mm:ss.fff");
        public int Posicao { get; set; } // 1 = Comprador, 2+ = Fila
        public string? InstagramCommentId { get; set; }
        public bool IsRetroativo { get; set; }
    }

    public class IniciarRastreioRequest
    {
        public int LiveId { get; set; }
        public long? LiveVideoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }

    public class ConfirmarArremateRequest
    {
        public int LiveId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? Arrematante { get; set; }
        public string? CompradorHoraTexto { get; set; }
        public List<MatchDto> Matches { get; set; } = new();
        public string? GoogleSheetUrl { get; set; }
        public string? SheetName { get; set; } = "vendas";
    }

    public class EstadoRastreioDto
    {
        public int LiveId { get; set; }
        public bool IsTracking { get; set; }
        public string? ActiveCode { get; set; }
        public string? ActiveDescription { get; set; }
        public decimal? ActiveValue { get; set; }
        public List<MatchDto> Matches { get; set; } = new();
    }

    public class AuditoriaMatchDto
    {
        public int Posicao { get; set; }
        public string Username { get; set; } = string.Empty;
        public string TextoDigitado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TimestampFmt => Timestamp.ToString("HH:mm:ss.fff");
        public string Status { get; set; } = string.Empty; // "🏆 Comprador", "📋 Fila 1", etc.
    }

    public class SsnMessageRequest
    {
        public int LiveId { get; set; }
        public string ChatName { get; set; } = string.Empty;
        public string ChatMessage { get; set; } = string.Empty;
        public string? Platform { get; set; }
        public long? TimestampMs { get; set; }
    }

    public class LiveAtivaInfo
    {
        public bool IsLive { get; set; }
        public long? LiveVideoId { get; set; }
        public int? LiveId { get; set; }
        public string? Titulo { get; set; }
        public int TotalComentarios { get; set; }
    }
}
