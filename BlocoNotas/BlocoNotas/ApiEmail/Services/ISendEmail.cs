namespace BlocoNotas.ApiEmail.Services;

/// <summary>
/// Interface que define o contrato para serviços de envio de emails.
/// Permite implementar diferentes estratégias de envio (SMTP, serviços externos, mocks para testes, etc.).
/// </summary>
public interface ISendEmail
{
    /// <summary>
    /// Envia um email de forma assíncrona.
    /// </summary>
    /// <param name="email">Destinatário do email</param>
    /// <param name="subject">Assunto do email</param>
    /// <param name="body">Conteúdo do email em HTML ou texto</param>
    Task SendEmailAsync(string email, string subject, string body);
}