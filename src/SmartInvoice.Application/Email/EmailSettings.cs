namespace SmartInvoice.Application.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool UseSsl { get; set; }
    public string FromAddress { get; set; } = "noreply@smartinvoice.dev";
    public string FromName { get; set; } = "Smart Invoice";
}
