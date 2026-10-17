using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Services
{
    /// <summary>
    /// Confirmação de pagamento do frete (webhook da InfinitePay ou botão "Marcar como pago")
    /// e liberação da etiqueta no Superfrete.
    /// </summary>
    public class PagamentoFreteService
    {
        public const string OrigemAutomatico = "Automatico";
        public const string OrigemManual = "Manual";

        private readonly AbrechozeiraContext _context;
        private readonly SuperfreteService _superfrete;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PagamentoFreteService> _logger;

        public PagamentoFreteService(
            AbrechozeiraContext context,
            SuperfreteService superfrete,
            IServiceScopeFactory scopeFactory,
            ILogger<PagamentoFreteService> logger)
        {
            _context = context;
            _superfrete = superfrete;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Marca a cotação como paga, registrando a origem (automático/manual), e libera a etiqueta se ela já existir.
        /// Sem etiqueta, o pagamento fica registrado e a liberação acontece quando a etiqueta for gerada.
        /// Devolve a mensagem de erro do Superfrete se a liberação falhar.
        /// </summary>
        public async Task<string?> ConfirmarPagamentoAsync(EnvioLoteMap map, string origem, string? usuario = null)
        {
            if (map.StatusPagamento != "Pago")
            {
                map.StatusPagamento = "Pago";
                map.OrigemPagamento = origem;
                map.PagoEm = DateTime.Now;
                map.PagoManualPor = origem == OrigemManual ? usuario : null;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Pagamento do frete {TransacaoId} confirmado ({Origem}).", map.TransacaoId, origem);
            }

            if (!string.IsNullOrEmpty(map.EtiquetaId) && map.StatusSuperfrete != "Liberada")
                return await LiberarEtiquetaAsync(map);
            return null;
        }

        /// <summary>
        /// Faz o checkout da etiqueta no Superfrete e, em seguida, envia o e-mail de rastreio em segundo plano.
        /// Devolve null em caso de sucesso ou a mensagem de erro do Superfrete.
        /// </summary>
        public async Task<string?> LiberarEtiquetaAsync(EnvioLoteMap map)
        {
            var (sucesso, erro) = await _superfrete.CheckoutAsync(map.EtiquetaId);
            if (!sucesso)
            {
                _logger.LogError("Erro ao processar checkout da etiqueta {EtiquetaId} no Superfrete após pagamento: {Erro}", map.EtiquetaId, erro);
                map.StatusSuperfrete = "Erro Checkout";
                await _context.SaveChangesAsync();
                return erro ?? "Erro desconhecido no Superfrete";
            }

            _logger.LogInformation("Checkout da etiqueta {EtiquetaId} realizado com sucesso!", map.EtiquetaId);
            map.StatusSuperfrete = "Liberada";
            await _context.SaveChangesAsync();

            var etiquetaId = map.EtiquetaId;
            var email = map.Email;
            var nome = map.Nome;
            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var superfrete = scope.ServiceProvider.GetRequiredService<SuperfreteService>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<PagamentoFreteService>>();

                try
                {
                    await Task.Delay(3000);
                    var info = await superfrete.ObterEtiquetaAsync(etiquetaId);
                    if (info != null && !string.IsNullOrWhiteSpace(info.Tracking) && !string.IsNullOrWhiteSpace(email))
                    {
                        await emailService.EnviarRastreioAsync(email, nome, info.Tracking, info.ServiceName);
                        logger.LogInformation("E-mail de rastreio automático enviado para {Email}.", email);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao enviar e-mail de rastreio pós-checkout.");
                }
            });

            return null;
        }
    }
}
