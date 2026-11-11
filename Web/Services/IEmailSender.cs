namespace Web.Services;

public interface IEmailSender
{
    Task SendConfirmationEmailAsync(string email, string confirmationUrl);
}
