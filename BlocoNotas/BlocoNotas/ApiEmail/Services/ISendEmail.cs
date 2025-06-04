namespace BlocoNotas.ApiEmail.Services;

public interface ISendEmail
{
    Task SendEmailAsync(string email, string subject, string body);
}