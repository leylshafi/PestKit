using PestKit.Interfaces;
using System.Net;
using System.Net.Mail;

namespace PestKit.Services
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string emailTo, string subject, string body, bool isHTML = false)
        {
            SmtpClient client = new SmtpClient(_configuration["Email:Host"], Convert.ToInt32(_configuration["Email:Port"]));
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_configuration["Email:LoginEmail"], _configuration["Email:Password"]);
            MailAddress from = new MailAddress(_configuration["Email:LoginEmail"]);
            MailAddress to = new MailAddress(emailTo);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHTML;
            await client.SendMailAsync(message);
        }
    }
}
