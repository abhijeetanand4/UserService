
        
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace UserService.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("abhijeetanand4269@gmail.com"));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                "abhijeetanand4269@gmail.com",
                "aomsgtvxgwvqabvs"
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
