using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABrechozeiraApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ABrechozeiraApp.Services;

/// <summary>
/// Serviço responsável pela conversão Pedido → Venda (padrão de mercado)
/// </summary>
public class VendaService
{
    private readonly AbrechozeiraContext _context;
    private readonly ILogger<VendaService> _logger;

    public VendaService(AbrechozeiraContext context, ILogger<VendaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gera uma Venda a partir de um Pedido confirmado
    /// </summary>
    public async Task<Venda> GerarVendaDePedidoAsync(int pedidoId, int? liveId = null, int? origemId = null)
    {
        var pedido = await _context.Pedido
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
            throw new ArgumentException($"Pedido {pedidoId} não encontrado.");

        // Verificar se já existe Venda para este Pedido
        var vendaExistente = await _context.Venda.FirstOrDefaultAsync(v => v.PedidoId == pedidoId);
        if (vendaExistente != null)
            throw new InvalidOperationException($"Já existe Venda (ID: {vendaExistente.Id}) para o Pedido {pedidoId}.");

        // Buscar itens do pedido para calcular valores
        var itensPedido = await _context.PedidoProduto
            .Where(pp => pp.PedidoId == pedidoId)
            .ToListAsync();

        if (!itensPedido.Any())
            throw new InvalidOperationException($"Pedido {pedidoId} não possui itens.");

        var valorBruto = itensPedido.Sum(i => (i.ValorFinalProduto ?? 0) * i.Quantidade);
        var descontoTotal = itensPedido.Sum(i => i.DescontoValor ?? 0);

        // Aplicar desconto percentual do pedido, se houver
        if (pedido.DescontoPorcentagem.HasValue && pedido.DescontoPorcentagem.Value > 0)
        {
            descontoTotal += valorBruto * (pedido.DescontoPorcentagem.Value / 100m);
        }

        var valorTotal = pedido.ValorTotal ?? (valorBruto - descontoTotal);

        var venda = new Venda
        {
            PedidoId = pedidoId,
            ClienteId = pedido.ClienteID,
            ValorBruto = valorBruto,
            Desconto = descontoTotal > 0 ? descontoTotal : null,
            ValorTotal = valorTotal,
            Status = "Confirmada",
            LiveId = liveId,
            OrigemID = origemId,
            FormaPagamentoId = pedido.FormaPagamentoID,
            CondicaoPagamentoId = pedido.CondicaoPagamentoID,
            DataVenda = DateTime.UtcNow,
            DataAlteracao = DateTime.UtcNow,
            Observacoes = pedido.Observacoes
        };

        _context.Venda.Add(venda);

        // Atualizar status do pedido para "Faturado" (se existir esse status)
        var statusFaturado = await _context.PedidoStatus
            .FirstOrDefaultAsync(s => s.Descricao.Contains("Faturado") || s.Descricao.Contains("faturado"));
        if (statusFaturado != null)
        {
            pedido.PedidoStatusID = statusFaturado.Id;
            pedido.DataAlteracao = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Venda {VendaId} gerada a partir do Pedido {PedidoId}. Valor: {ValorTotal}",
            venda.Id, pedidoId, valorTotal);

        return venda;
    }

    /// <summary>
    /// Gera Vendas em lote para vários Pedidos
    /// </summary>
    public async Task<List<Venda>> GerarVendasEmLoteAsync(List<int> pedidoIds, int? liveId = null, int? origemId = null)
    {
        var vendas = new List<Venda>();
        foreach (var pedidoId in pedidoIds)
        {
            try
            {
                var venda = await GerarVendaDePedidoAsync(pedidoId, liveId, origemId);
                vendas.Add(venda);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao gerar venda para pedido {PedidoId}: {Erro}", pedidoId, ex.Message);
            }
        }
        return vendas;
    }
}
