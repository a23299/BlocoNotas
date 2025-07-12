using BlocoNotas.ApiEmail.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNotas.ApiEmail.Controllers;

/// <summary>
/// Controlador responsável pelo envio de emails.
/// </summary>
[Route("[controller]")]
[ApiController]
public class EmailsController : ControllerBase
{
    private readonly ILogger<EmailsController> _logger;
    private readonly ISendEmail _sendEmail;

    /// <summary>
    /// Injeta o serviço de logging e de envio de email.
    /// </summary>
    public EmailsController(ILogger<EmailsController> logger, ISendEmail sendEmail)
    {
        _logger = logger;
        _sendEmail = sendEmail;
    }

    /// <summary>
    /// Endpoint GET para testar o envio de email.
    /// Requer os parâmetros: email, subject e body.
    /// </summary>
    /// <param name="email">Endereço de email de destino</param>
    /// <param name="subject">Assunto do email</param>
    /// <param name="body">Conteúdo do email</param>
    /// <returns>200 OK se o email for enviado, 400 BadRequest em caso de erro</returns>
    [HttpGet("sendemail")]
    public async Task<IActionResult> TestEmail(string email, string subject, string body)
    {
        // Validação mínima dos dados recebidos
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning("Dados inválidos fornecidos para envio de email.");
            return BadRequest("Email, assunto e conteúdo são obrigatórios.");
        }

        try
        {
            await _sendEmail.SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email enviado com sucesso para {Email}", email);
            return Ok("Email enviado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {Email}", email);
            return BadRequest("Erro ao enviar o email.");
        }
    }
}