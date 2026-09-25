using System.Net;
using System.Net.Mail;

namespace ABrechozeiraApp.Services
{
    /// <summary>
    /// Serviço de envio de e-mails para notificar clientes sobre cotações e rastreio.
    /// Usa SMTP configurado no appsettings.json.
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ─── Enviar Cotação ──────────────────────────────────────────────────────

        /// <summary>
        /// Envia e-mail ao cliente com o valor cotado do frete.
        /// </summary>
        public async Task<bool> EnviarCotacaoAsync(
            string destinatarioEmail,
            string nomeCliente,
            string servicoNome,
            decimal precoFrete,
            decimal precoPac,
            decimal precoSedex,
            string checkoutUrl)
        {
            if (string.IsNullOrWhiteSpace(destinatarioEmail))
            {
                _logger.LogWarning("E-mail de cotação não enviado: destinatário vazio para {Nome}", nomeCliente);
                return false;
            }

            var assunto = "🛍️ A Brechozeira — Cotação de Frete do seu Pedido";

            var corpo = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
</head>
<body style=""font-family: Arial, Helvetica, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        
        <!-- Header -->
        <div style=""background-color: #d4a0a0; padding: 24px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 22px; letter-spacing: 1px;"">A Brechozeira</h1>
            <p style=""color: #fff8f8; margin: 4px 0 0; font-size: 13px;"">Brechó e Outlet</p>
        </div>

        <!-- Corpo -->
        <div style=""padding: 28px 24px;"">
            <p style=""font-size: 15px; color: #333; margin-bottom: 20px;"">
                Olá, <strong>{WebUtility.HtmlEncode(nomeCliente)}</strong>! 👋
            </p>
            
            <p style=""font-size: 14px; color: #555; line-height: 1.6;"">
                Cotamos o frete do seu pedido e aqui estão os detalhes:
            </p>

            <!-- Card do frete -->
            <div style=""background-color: #faf6f6; border: 1px solid #e8d5d5; border-radius: 8px; padding: 20px; margin: 20px 0;"">
                <table style=""width: 100%; font-size: 14px; color: #444;"">
                    <tr>
                        <td style=""padding: 6px 0; font-weight: bold;"">📦 Serviço:</td>
                        <td style=""padding: 6px 0; text-align: right;"">{WebUtility.HtmlEncode(servicoNome)}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 6px 0; font-weight: bold;"">💰 Valor do Frete:</td>
                        <td style=""padding: 6px 0; text-align: right; font-size: 18px; color: #c0392b; font-weight: bold;"">
                            R$ {precoFrete:F2}
                        </td>
                    </tr>
                </table>
                <div style=""margin-top: 12px; padding-top: 12px; border-top: 1px dashed #e8d5d5; font-size: 13px; color: #777; text-align: right;"">
                    {(servicoNome == "PAC" ? $"Sedex: R$ {precoSedex:F2}" : $"PAC: R$ {precoPac:F2}")}
                </div>
            </div>

            <!-- Card do PIX (InfinitePay) -->
            <div style=""background-color: #fcf8f8; border: 1px solid #e8d5d5; border-radius: 8px; padding: 20px; margin: 20px 0; text-align: center;"">
                <p style=""font-size: 14px; color: #444; margin: 0 0 16px; font-weight: bold;"">📲 Efetue o pagamento do frete</p>
                <p style=""font-size: 13px; color: #555; margin: 0 0 20px;"">Clique no botão abaixo para pagar via PIX ou Cartão usando a InfinitePay. A confirmação é automática!</p>
                
                <a href=""{checkoutUrl}"" style=""display: inline-block; background-color: #17e28d; color: #000; font-weight: bold; font-size: 16px; padding: 14px 24px; border-radius: 8px; text-decoration: none; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    Pagar Frete Agora
                </a>
            </div>

            <p style=""font-size: 14px; color: #555; margin-top: 24px;"">
                Se tiver dúvidas, é só nos chamar! 💜
            </p>
        </div>

        <!-- Footer -->
        <div style=""background-color: #f9f3f3; padding: 16px 24px; text-align: center; border-top: 1px solid #e8d5d5;"">
            <p style=""font-size: 12px; color: #999; margin: 0;"">
                A Brechozeira Brechó e Outlet<br>
                R. Nicola Pelanda, 1117 – Loja 4 – Pinheirinho, Curitiba/PR
            </p>
        </div>
    </div>
</body>
</html>";

            return await EnviarEmailAsync(destinatarioEmail, assunto, corpo);
        }

        // ─── Enviar Rastreio ─────────────────────────────────────────────────────

        /// <summary>
        /// Envia e-mail ao cliente com o código de rastreio e link de rastreamento.
        /// </summary>
        public async Task<bool> EnviarRastreioAsync(
            string destinatarioEmail,
            string nomeCliente,
            string codigoRastreio,
            string servicoNome)
        {
            if (string.IsNullOrWhiteSpace(destinatarioEmail))
            {
                _logger.LogWarning("E-mail de rastreio não enviado: destinatário vazio para {Nome}", nomeCliente);
                return false;
            }

            if (string.IsNullOrWhiteSpace(codigoRastreio))
            {
                _logger.LogWarning("E-mail de rastreio não enviado: código de rastreio vazio para {Nome}", nomeCliente);
                return false;
            }

            var linkRastreamento = $"https://rastreamento.superfrete.com/#{codigoRastreio}";
            var assunto = "📦 A Brechozeira — Seu pedido foi enviado! Rastreie aqui";

            var corpo = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
</head>
<body style=""font-family: Arial, Helvetica, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        
        <!-- Header -->
        <div style=""background-color: #d4a0a0; padding: 24px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 22px; letter-spacing: 1px;"">A Brechozeira</h1>
            <p style=""color: #fff8f8; margin: 4px 0 0; font-size: 13px;"">Brechó e Outlet</p>
        </div>

        <!-- Corpo -->
        <div style=""padding: 28px 24px;"">
            <p style=""font-size: 15px; color: #333; margin-bottom: 20px;"">
                Olá, <strong>{WebUtility.HtmlEncode(nomeCliente)}</strong>! 🎉
            </p>
            
            <p style=""font-size: 14px; color: #555; line-height: 1.6;"">
                Ótima notícia! Seu pedido já foi <strong>postado</strong> e está a caminho! 🚚
            </p>

            <!-- Card do rastreio -->
            <div style=""background-color: #f0faf0; border: 1px solid #c8e6c9; border-radius: 8px; padding: 20px; margin: 20px 0; text-align: center;"">
                <p style=""font-size: 13px; color: #666; margin: 0 0 8px;"">Código de Rastreio:</p>
                <p style=""font-size: 22px; font-weight: bold; color: #2e7d32; margin: 0 0 4px; letter-spacing: 2px;"">
                    {WebUtility.HtmlEncode(codigoRastreio)}
                </p>
                <p style=""font-size: 12px; color: #888; margin: 0 0 16px;"">
                    Enviado via {WebUtility.HtmlEncode(servicoNome)}
                </p>
                <a href=""{linkRastreamento}"" 
                   style=""display: inline-block; background-color: #2e7d32; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 6px; font-size: 14px; font-weight: bold;"">
                    📍 Rastrear meu pedido
                </a>
            </div>

            <p style=""font-size: 13px; color: #888; line-height: 1.5;"">
                Você também pode copiar o código acima e colar diretamente no site dos Correios para acompanhar.
            </p>

            <p style=""font-size: 14px; color: #555; margin-top: 24px;"">
                Obrigada pela compra! Qualquer dúvida, estamos por aqui. 💜
            </p>
        </div>

        <!-- Footer -->
        <div style=""background-color: #f9f3f3; padding: 16px 24px; text-align: center; border-top: 1px solid #e8d5d5;"">
            <p style=""font-size: 12px; color: #999; margin: 0;"">
                A Brechozeira Brechó e Outlet<br>
                R. Nicola Pelanda, 1117 – Loja 4 – Pinheirinho, Curitiba/PR
            </p>
        </div>
    </div>
</body>
</html>";

            return await EnviarEmailAsync(destinatarioEmail, assunto, corpo);
        }

        // ─── Enviar Link de Acesso ao Perfil ────────────────────────────────────

        /// <summary>
        /// Envia e-mail ao cliente com link de acesso rápido (Magic Link) e/ou senha temporária para gerenciar cadastro.
        /// </summary>
        public async Task<bool> EnviarLinkAcessoPerfilAsync(
            string destinatarioEmail,
            string nomeCliente,
            string linkAcesso,
            string? senhaTemporaria = null)
        {
            if (string.IsNullOrWhiteSpace(destinatarioEmail))
            {
                _logger.LogWarning("E-mail de acesso não enviado: destinatário vazio para {Nome}", nomeCliente);
                return false;
            }

            var assunto = "🔐 A Brechozeira — Acesso ao seu cadastro";

            var blocoSenha = !string.IsNullOrWhiteSpace(senhaTemporaria) ? $@"
            <div style=""background-color: #faf6f6; border: 1px dashed #d4a0a0; border-radius: 8px; padding: 14px; margin: 18px 0; text-align: center;"">
                <p style=""font-size: 13px; color: #666; margin: 0 0 6px;"">Caso prefira entrar digitando a senha:</p>
                <p style=""font-size: 18px; font-weight: bold; color: #a34e4e; letter-spacing: 2px; margin: 0; font-family: monospace;"">{WebUtility.HtmlEncode(senhaTemporaria)}</p>
            </div>" : "";

            var corpo = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
</head>
<body style=""font-family: Arial, Helvetica, sans-serif; background-color: #f7f3f3; margin: 0; padding: 20px;"">
    <div style=""max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08);"">
        
        <!-- Header -->
        <div style=""background: linear-gradient(135deg, #d4a0a0 0%, #b87a7a 100%); padding: 28px 24px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; letter-spacing: 1.5px; font-weight: 600;"">A Brechozeira</h1>
            <p style=""color: #fff0f0; margin: 6px 0 0; font-size: 13px; letter-spacing: 0.5px;"">Portal do Cliente • Brechó e Outlet</p>
        </div>

        <!-- Corpo -->
        <div style=""padding: 32px 28px;"">
            <p style=""font-size: 16px; color: #333; margin-top: 0; margin-bottom: 16px;"">
                Olá, <strong>{WebUtility.HtmlEncode(nomeCliente)}</strong>! 👋
            </p>
            
            <p style=""font-size: 14px; color: #555; line-height: 1.6; margin-bottom: 24px;"">
                Você solicitou acesso ao portal da <strong>A Brechozeira</strong> para consultar ou atualizar suas informações cadastrais e endereço de entrega.
            </p>

            <!-- Card de Ação Principal -->
            <div style=""background-color: #faf5f5; border: 1px solid #ebdada; border-radius: 8px; padding: 24px 20px; text-align: center; margin: 24px 0;"">
                <p style=""font-size: 14px; color: #444; margin: 0 0 16px; font-weight: bold;"">
                    Clique no botão abaixo para acessar seus dados diretamente:
                </p>
                <a href=""{linkAcesso}"" 
                   style=""display: inline-block; background-color: #c97d7d; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; font-size: 15px; font-weight: bold; letter-spacing: 0.5px; box-shadow: 0 3px 8px rgba(201, 125, 125, 0.35);"">
                    ✨ Acessar e Alterar Meus Dados
                </a>
                <p style=""font-size: 12px; color: #888; margin: 16px 0 0;"">
                    🔒 Este link de acesso seguro é válido por <strong>2 horas</strong>.
                </p>
            </div>

            {blocoSenha}

            <p style=""font-size: 13px; color: #777; line-height: 1.5; margin-top: 24px;"">
                Se você não solicitou este acesso, por favor desconsidere este e-mail. Seus dados permanecem em segurança.
            </p>

            <p style=""font-size: 14px; color: #555; margin-top: 24px; margin-bottom: 0;"">
                Com carinho,<br>
                <strong>Equipe A Brechozeira 💜</strong>
            </p>
        </div>

        <!-- Footer -->
        <div style=""background-color: #fcf9f9; padding: 18px 24px; text-align: center; border-top: 1px solid #eee;"">
            <p style=""font-size: 12px; color: #aaa; margin: 0; line-height: 1.5;"">
                A Brechozeira Brechó e Outlet<br>
                R. Nicola Pelanda, 1117 – Loja 4 – Pinheirinho, Curitiba/PR
            </p>
        </div>
    </div>
</body>
</html>";

            return await EnviarEmailAsync(destinatarioEmail, assunto, corpo);
        }

        // ─── Engine SMTP ─────────────────────────────────────────────────────────

        private async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            try
            {
                var smtpHost = _config["Email:SmtpHost"] ?? "smtp.kinghost.net";
                var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
                var smtpUser = _config["Email:SmtpUser"] ?? "";
                var smtpPassword = _config["Email:SmtpPassword"] ?? "";
                var fromEmail = _config["Email:FromEmail"] ?? smtpUser;
                var fromName = _config["Email:FromName"] ?? "A Brechozeira";
                var enableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "true");

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000 // 15 segundos
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = assunto,
                    Body = corpoHtml,
                    IsBodyHtml = true,
                    SubjectEncoding = System.Text.Encoding.UTF8,
                    BodyEncoding = System.Text.Encoding.UTF8
                };

                message.To.Add(new MailAddress(destinatario));

                await client.SendMailAsync(message);

                _logger.LogInformation("📧 E-mail enviado com sucesso para {Destinatario} | Assunto: {Assunto}", destinatario, assunto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao enviar e-mail para {Destinatario}", destinatario);
                return false;
            }
        }
    }
}
