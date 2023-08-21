using System.Net.Mail;
using System.Net;
namespace AuthSystem.Utils
{
    public class EmailSender
    {
        public void Send(string sender , string reciptent , string subject , string body) {
            var emailSender = new SmtpClient("smtp.office365.com", 587)
            {
                Credentials = new NetworkCredential("donotreply@paf-iast.edu.pk", "PAF@2024"),
                EnableSsl = true
            };
            emailSender.Send(sender, reciptent, subject, body);
        }
    }
}
