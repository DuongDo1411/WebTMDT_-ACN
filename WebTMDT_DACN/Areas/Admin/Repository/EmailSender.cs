using System.Net;
using System.Net.Mail;

namespace WebTMDT_DACN.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("duongdokimma@gmail.com", "upnyisjjhktmxuay")
            };

            return client.SendMailAsync(
                new MailMessage(from: "duongdokimma@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
