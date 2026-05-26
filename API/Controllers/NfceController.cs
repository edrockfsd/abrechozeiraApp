using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using ABrechozeiraApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace ABrechozeiraApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NfceController : ControllerBase
{
    private readonly AbrechozeiraContext _context;
    private readonly NfceService _nfceService;

    public NfceController(AbrechozeiraContext context, NfceService nfceService)
    {
        _context = context;
        _nfceService = nfceService;
    }

    #region Configuração

    /// <summary>
    /// Obtém as configurações fiscais da empresa
    /// </summary>
    [HttpGet("config")]
    public async Task<ActionResult<EmpresaFiscal>> GetConfig()
    {
        var config = await _nfceService.GetConfigAsync();
        if (config == null)
            return NotFound("Configurações fiscais não encontradas.");

        // Não retornar dados sensíveis
        config.CertificadoSenha = null;

        return Ok(config);
    }

    /// <summary>
    /// Salva ou atualiza as configurações fiscais
    /// </summary>
    [HttpPost("config")]
    public async Task<ActionResult<EmpresaFiscal>> SaveConfig([FromBody] EmpresaFiscal config)
    {
        var existente = await _context.EmpresaFiscal.FirstOrDefaultAsync(e => e.Ativo);

        if (existente == null)
        {
            config.Id = 0;
            config.DataCriacao = DateTime.UtcNow;
            config.DataAlteracao = DateTime.UtcNow;
            config.Ativo = true;
            _context.EmpresaFiscal.Add(config);
        }
        else
        {
            existente.CNPJ = config.CNPJ;
            existente.InscricaoEstadual = config.InscricaoEstadual;
            existente.RazaoSocial = config.RazaoSocial;
            existente.NomeFantasia = config.NomeFantasia;
            existente.Logradouro = config.Logradouro;
            existente.Numero = config.Numero;
            existente.Complemento = config.Complemento;
            existente.Bairro = config.Bairro;
            existente.CodigoMunicipio = config.CodigoMunicipio;
            existente.Municipio = config.Municipio;
            existente.UF = config.UF;
            existente.CEP = config.CEP;
            existente.Telefone = config.Telefone;
            existente.Ambiente = config.Ambiente;
            existente.CRT = config.CRT;
            existente.Serie = config.Serie;
            existente.TipoEmissao = config.TipoEmissao;
            existente.CSC = config.CSC;
            existente.CSCId = config.CSCId;

            // Atualizar certificado apenas se informado
            if (!string.IsNullOrEmpty(config.CertificadoPath))
            {
                existente.CertificadoPath = config.CertificadoPath;
            }
            if (!string.IsNullOrEmpty(config.CertificadoSenha))
            {
                existente.CertificadoSenha = config.CertificadoSenha;
            }
            existente.CertificadoValidade = config.CertificadoValidade;

            existente.DataAlteracao = DateTime.UtcNow;
            config = existente;
        }

        await _context.SaveChangesAsync();
        config.CertificadoSenha = null;

        return Ok(config);
    }

    /// <summary>
    /// Valida se a configuração está completa
    /// </summary>
    [HttpGet("config/validar")]
    public async Task<ActionResult<object>> ValidarConfig()
    {
        var (valido, erro) = await _nfceService.ValidarConfiguracaoAsync();
        return Ok(new { valido, erro });
    }

    #endregion

    #region Emissão

    /// <summary>
    /// Emite NFC-e para uma venda do PDV
    /// </summary>
    [HttpPost("emitir/venda/{vendaPdvId}")]
    public async Task<ActionResult<Nfce>> EmitirVendaPdv(int vendaPdvId)
    {
        try
        {
            var nfce = await _nfceService.EmitirNfceVendaPdvAsync(vendaPdvId);
            return Ok(nfce);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// Emite NFC-e para um pedido
    /// </summary>
    [HttpPost("emitir/pedido/{pedidoId}")]
    public async Task<ActionResult<Nfce>> EmitirPedido(int pedidoId)
    {
        try
        {
            var nfce = await _nfceService.EmitirNfcePedidoAsync(pedidoId);
            return Ok(nfce);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }

    #endregion

    #region Consulta

    /// <summary>
    /// Lista NFC-e com filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> Listar(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim,
        [FromQuery] string? status,
        [FromQuery] int limite = 50)
    {
        var lista = await _nfceService.ListarNfceAsync(inicio, fim, status, limite);
        return Ok(lista.Select(n => new
        {
            n.Id,
            n.Numero,
            n.Serie,
            n.ChaveAcesso,
            n.Status,
            n.ValorTotal,
            n.DataEmissao,
            n.DataAutorizacao,
            n.VendaPdvId,
            n.PedidoId
        }));
    }

    /// <summary>
    /// Obtém detalhes de uma NFC-e
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Nfce>> GetById(int id)
    {
        var nfce = await _nfceService.GetNfceAsync(id);
        if (nfce == null)
            return NotFound();

        return Ok(nfce);
    }

    /// <summary>
    /// Obtém NFC-e por chave de acesso
    /// </summary>
    [HttpGet("chave/{chaveAcesso}")]
    public async Task<ActionResult<Nfce>> GetByChave(string chaveAcesso)
    {
        var nfce = await _context.Nfce
            .Include(n => n.Itens)
            .Include(n => n.Pagamentos)
            .FirstOrDefaultAsync(n => n.ChaveAcesso == chaveAcesso);

        if (nfce == null)
            return NotFound();

        return Ok(nfce);
    }

    #endregion

    #region Cancelamento

    /// <summary>
    /// Cancela uma NFC-e
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult<Nfce>> Cancelar(int id, [FromBody] CancelarNfceRequest request)
    {
        try
        {
            var nfce = await _nfceService.CancelarNfceAsync(id, request.Justificativa);
            return Ok(nfce);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    #endregion

    #region DANFE

    /// <summary>
    /// Gera o DANFE (PDF) de uma NFC-e
    /// </summary>
    [HttpGet("{id}/danfe")]
    public async Task<ActionResult> GetDanfe(int id)
    {
        var nfce = await _nfceService.GetNfceAsync(id);
        if (nfce == null)
            return NotFound();

        // TODO: Implementar geração de DANFE com ACBrLib
        // Por enquanto, retorna informação básica
        return Ok(new
        {
            mensagem = "DANFE será gerado quando a integração com ACBrLib estiver completa.",
            nfce = new
            {
                nfce.Id,
                nfce.Numero,
                nfce.Serie,
                nfce.ChaveAcesso,
                nfce.Status,
                nfce.ValorTotal,
                nfce.DataEmissao
            }
        });
    }

    #endregion
}

public class CancelarNfceRequest
{
    public string Justificativa { get; set; } = string.Empty;
}
