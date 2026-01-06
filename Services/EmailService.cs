using MailKit.Net.Smtp;
using MimeKit;

namespace UserService.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Implementation for sending email
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("noreply@yourapp.com"));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, false);
            await smtp.AuthenticateAsync("your-email@gmail.com", "app-password");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
