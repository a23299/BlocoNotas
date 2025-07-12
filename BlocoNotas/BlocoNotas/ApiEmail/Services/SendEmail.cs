using BlocoNotas.ApiEmail.Entities;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BlocoNotas.ApiEmail.Services;

/// <summary>
/// Implementação do serviço de envio de email usando MailKit.
/// </summary>
public class SendEmail : ISendEmail
{
    private readonly SmtpSettings _smtpSettings;
    private readonly IWebHostEnvironment _env;

    public SendEmail(IOptions<SmtpSettings> smtpSettings, IWebHostEnvironment env)
    {
        _smtpSettings = smtpSettings.Value;
        _env = env;
    }

    /// <summary>
    /// Envia um email para o destinatário especificado.
    /// </summary>
    /// <param name="email">Email de destino</param>
    /// <param name="subject">Assunto</param>
    /// <param name="body">Conteúdo HTML</param>
    public async Task SendEmailAsync(string email, string subject, string body)
    {
        // Criação da mensagem
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        // Envio via SMTP
        using var client = new SmtpClient();

        // Validar o certificado
        client.ServerCertificateValidationCallback = (s, c, h, e) => _env.IsDevelopment();

        try
        {
            // Ligação segura
            await client.ConnectAsync(
                _smtpSettings.Server,
                _smtpSettings.Port,
                MailKit.Security.SecureSocketOptions.StartTls);

            // Autenticação
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

            // Envio
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            // Usa logger se quiseres, em vez de lançar exceção genérica
            throw new InvalidOperationException("Erro ao enviar email: " + ex.Message, ex);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
