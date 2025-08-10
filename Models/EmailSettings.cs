namespace FEENALOoFINALE.Models
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool EnableEmails { get; set; } = true;
        public bool TestMode { get; set; } = false;
        public string TestEmailRecipient { get; set; } = string.Empty;
    }
}
