using SendGrid;
using SendGrid.Helpers.Mail;

namespace SignupIdentity
{
    public class EmailService(IConfiguration configuration)
    {
        public async Task SendEmailConfirmation(string email, string confirmationLink)
        {
            var client = new SendGridClient(configuration["SendGridApiKey"]);
            var from = new EmailAddress("YOUR_SENDGRID_EMAIL");
            var subject = "Account Confirmation";
            var to = new EmailAddress(email);
            var plainTextContent = confirmationLink;
            var htmlContent = string.Empty;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            
            await client.SendEmailAsync(msg);
        }
    }
}
