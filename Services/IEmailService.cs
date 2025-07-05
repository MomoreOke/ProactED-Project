namespace FEENALOoFINALE.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailVerificationAsync(string email, string verificationUrl);
        bool IsValidEmail(string email);
    }
}
