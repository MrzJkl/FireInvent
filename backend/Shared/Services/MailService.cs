using FlameGuardLaundry.Shared.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FlameGuardLaundry.Shared.Services
{
    public class MailService(IOptions<MailOptions> mailOptions) : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (mailOptions.Value == null || 
                string.IsNullOrWhiteSpace(mailOptions.Value.Username) || 
                string.IsNullOrWhiteSpace(mailOptions.Value.Password) ||
                string.IsNullOrWhiteSpace(mailOptions.Value.Host) || 
                string.IsNullOrWhiteSpace(mailOptions.Value.SenderName) || 
                string.IsNullOrWhiteSpace(mailOptions.Value.SenderAddress))
                throw new ArgumentNullException(nameof(mailOptions), "Mail options must be configured to be able to send mails.");


            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(mailOptions.Value.SenderName, mailOptions.Value.SenderAddress));

            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;

            emailMessage.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                mailOptions.Value.Host,
                mailOptions.Value.Port,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                mailOptions.Value.Username,
                mailOptions.Value.Password);

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
