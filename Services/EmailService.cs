using MailKit.Net.Smtp;
using MimeKit;
using System.Text.RegularExpressions;

namespace FEENALOoFINALE.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                // Check if we're in development mode and email is not configured
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var password = _configuration["EmailSettings:Password"];
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                
                // Check if email service is properly configured
                bool isEmailConfigured = !string.IsNullOrEmpty(smtpHost) && 
                                        !string.IsNullOrEmpty(senderEmail) && 
                                        !string.IsNullOrEmpty(password) && 
                                        password != "demo-password" && 
                                        password != "your-app-password" &&
                                        !senderEmail.Contains("your-email") && 
                                        !senderEmail.Contains("demo");

                if (!isEmailConfigured)
                {
                    if (isDevelopment)
                    {
                        _logger.LogInformation($"Development mode: Simulating email send to {email}");
                        _logger.LogInformation($"EMAIL SIMULATION - To: {email}, Subject: {subject}");
                        _logger.LogInformation($"EMAIL CONTENT: {message}");
                        return; // Successfully "sent" in development mode
                    }
                    else
                    {
                        throw new InvalidOperationException("Email service is not properly configured for production environment.");
                    }
                }

                var emailMessage = new MimeMessage();
                
                emailMessage.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:SenderName"] ?? "ProactED System",
                    senderEmail
                ));
                
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");

                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}. Error: {ex.Message}");
                // Don't throw in development - just log the error
                if (_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Production")
                {
                    throw;
                }
            }
        }

        public async Task SendEmailVerificationAsync(string email, string verificationUrl)
        {
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            
            var subject = "Verify Your Email Address - ProactED";
            var message = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 28px;'>Welcome to ProactED!</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Predictive Maintenance System</p>
                        </div>
                        
                        <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
                            <h2 style='color: #2c3e50; margin-top: 0;'>Verify Your Email Address</h2>
                            <p>Thank you for registering with ProactED! To complete your account setup, please verify your email address by clicking the button below:</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{verificationUrl}' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; font-size: 16px;'>
                                    Verify Email Address
                                </a>
                            </div>
                            
                            <p style='font-size: 14px; color: #6c757d;'>
                                If the button doesn't work, copy and paste this link into your browser:<br>
                                <a href='{verificationUrl}' style='color: #667eea; word-break: break-all;'>{verificationUrl}</a>
                            </p>
                            
                            {(isDevelopment ? @"
                            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <strong style='color: #856404;'>Development Mode:</strong> 
                                <span style='color: #856404;'>This is a simulated email. You can also use the bypass verification feature on the login page.</span>
                            </div>" : "")}
                            
                            <hr style='border: none; border-top: 1px solid #dee2e6; margin: 30px 0;'>
                            
                            <p style='font-size: 14px; color: #6c757d; margin: 0;'>
                                If you didn't create an account with ProactED, please ignore this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, message);
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Comprehensive email validation regex
                var emailRegex = new Regex(
                    @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled
                );

                return emailRegex.IsMatch(email) && email.Length <= 254;
            }
            catch
            {
                return false;
            }
        }
    }
}
