using System.ComponentModel.DataAnnotations;

namespace ABrechozeiraApp.Models
{
    public partial class ProdutoPerfil
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Descricao { get; set; } = null!;
    }
}
