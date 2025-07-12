namespace BlocoNotas.ApiEmail.Entities;

/// <summary>
/// Representa as configurações do servidor SMTP para envio de emails.
/// Essas configurações são populadas via appsettings.json ou variáveis de ambiente.
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// Endereço do servidor SMTP (ex: smtp.gmail.com)
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Porta do servidor SMTP (ex: 587 para TLS, 465 para SSL)
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Nome do remetente exibido no email
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// Email do remetente usado no campo "From"
    /// </summary>
    public string? SenderEmail { get; set; }

    /// <summary>
    /// Username utilizado para autenticação no servidor SMTP (geralmente o próprio email)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Palavra-passe para autenticação SMTP (recomenda-se uso via variáveis de ambiente)
    /// </summary>
    public string? Password { get; set; }
}