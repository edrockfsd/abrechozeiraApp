namespace AbiaApp.API.Models // <--- Ajustado para seu projeto
{
	public class PedidoCadastro
	{
		public string TextoUsuario { get; set; }
        public string? OrigemFixa { get; set; } // <--- NOVO CAMPO
    }

	public class ProdutoIA_IDs
	{
		public string Descricao { get; set; }
		public string Tamanho { get; set; }
		public decimal PrecoVenda { get; set; }
		public decimal PrecoCusto { get; set; }

		// Valor padrão garante que não seja nulo
		public string Origem { get; set; } = "IA_Voice";

		public int MarcaId { get; set; }
		public int GrupoId { get; set; }
		public int GeneroId { get; set; }
		public int PerfilId { get; set; }
		public string Condicao { get; set; }
	}
}