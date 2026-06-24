using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models
{
    [Table("tb_enviolote_map")]
    public class EnvioLoteMap
    {
        [Key]
        [StringLength(255)]
        public string TransacaoId { get; set; } = string.Empty;

        [StringLength(255)]
        public string EtiquetaId { get; set; } = string.Empty;

        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(50)]
        public string StatusPagamento { get; set; } = "Aguardando";

        [StringLength(50)]
        public string StatusSuperfrete { get; set; } = "Carrinho";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrecoPAC { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrecoSEDEX { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrecoRecomendado { get; set; }

        [StringLength(50)]
        public string? ServicoRecomendado { get; set; }

        public string? LinkCheckout { get; set; }

        public bool EmailCotacaoEnviado { get; set; } = false;
        public bool EmailRastreioEnviado { get; set; } = false;
        public bool WhatsAppCotacaoEnviado { get; set; } = false;
        public bool WhatsAppRastreioEnviado { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
