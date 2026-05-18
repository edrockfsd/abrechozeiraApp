using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

/// <summary>
/// Pagamento da Nota Fiscal de Consumidor Eletrônica
/// </summary>
public class NfcePagamento
{
    public int Id { get; set; }

    [ForeignKey("Nfce")]
    public int NfceId { get; set; }
    public Nfce? Nfce { get; set; }

    /// <summary>
    /// Forma de pagamento conforme tabela SEFAZ:
    /// 01=Dinheiro, 02=Cheque, 03=Cartão de Crédito, 04=Cartão de Débito,
    /// 05=Crédito Loja, 10=Vale Alimentação, 11=Vale Refeição, 12=Vale Presente,
    /// 13=Vale Combustível, 15=Boleto Bancário, 16=Depósito Bancário,
    /// 17=PIX, 18=Transferência, 19=Cashback, 90=Sem Pagamento, 99=Outros
    /// </summary>
    [StringLength(2)]
    public string TipoPagamento { get; set; } = "01";

    public decimal Valor { get; set; }

    /// <summary>
    /// Tipo de integração com TEF:
    /// 1=Integrado, 2=Não integrado
    /// </summary>
    public int TipoIntegracao { get; set; } = 2;

    // Dados do cartão (quando aplicável)
    [StringLength(2)]
    public string? BandeiraCartao { get; set; }

    [StringLength(20)]
    public string? AutorizacaoCartao { get; set; }

    [StringLength(14)]
    public string? CNPJCredenciadora { get; set; }
}
