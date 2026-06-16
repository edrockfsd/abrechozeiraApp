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

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
