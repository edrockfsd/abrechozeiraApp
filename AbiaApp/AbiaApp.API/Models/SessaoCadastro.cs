namespace AbiaApp.API.Models
{
    /// <summary>
    /// Representa um item cadastrado durante a sessão (ainda não salvo no banco)
    /// </summary>
    public class ItemSessao
    {
        public int CodigoEstoque { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Tamanho { get; set; } = string.Empty;
        public decimal PrecoVenda { get; set; }
        public decimal PrecoCusto { get; set; }
        public string Origem { get; set; } = "IA_Voice";
        public int MarcaId { get; set; }
        public int GrupoId { get; set; }
        public int GeneroId { get; set; }
        public int PerfilId { get; set; }
        public string Condicao { get; set; } = "U";
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Representa uma sessão de cadastro em andamento
    /// </summary>
    public class SessaoCadastro
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public DateTime Inicio { get; set; } = DateTime.Now;
        public int ProximoCodigo { get; set; }
        public string? OrigemFixa { get; set; }
        public List<ItemSessao> Itens { get; set; } = new();
    }
}
