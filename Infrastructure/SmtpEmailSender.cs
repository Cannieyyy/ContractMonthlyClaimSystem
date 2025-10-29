using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Infrastructure
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpEmailSender(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var smtp = _config.GetSection("Smtp");
            var host = smtp.GetValue<string>("Host");
            var port = smtp.GetValue<int>("Port");
            var user = smtp.GetValue<string>("User");
            var pass = smtp.GetValue<string>("Pass");
            var from = smtp.GetValue<string>("From");
            var enableSsl = smtp.GetValue<bool>("EnableSsl");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = string.IsNullOrEmpty(user) ? null : new NetworkCredential(user, pass)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(to);
            await client.SendMailAsync(mail);                
        }
    }
}
                                                     