using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace Web.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendConfirmationEmailAsync(string email, string confirmationUrl)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.FromAddress) ||
            string.IsNullOrWhiteSpace(_options.PublicBaseUrl) ||
            _options.SmtpPort is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "Email settings are incomplete. Configure Email:PublicBaseUrl, Email:SmtpHost, Email:SmtpPort, and Email:FromAddress.");
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpUserName) !=
            string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            throw new InvalidOperationException(
                "Configure both Email:SmtpUserName and Email:SmtpPassword, or leave both empty for an unauthenticated SMTP relay.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = "Confirma tu correo electrónico",
            IsBodyHtml = true,
            Body = $"""
                <p>Gracias por registrarte en el blog.</p>
                <p><a href="{HtmlEncoder.Default.Encode(confirmationUrl)}">Confirma tu correo electrónico</a></p>
                <p>Si no creaste esta cuenta, puedes ignorar este mensaje.</p>
                """
        };
        message.To.Add(new MailAddress(email));

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpEnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUserName))
        {
            client.Credentials = new NetworkCredential(
                _options.SmtpUserName,
                _options.SmtpPassword);
        }

        await client.SendMailAsync(message);
    }
}
