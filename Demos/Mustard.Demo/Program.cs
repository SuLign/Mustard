using Mustard.Base.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mustard.Demo
{
    public class Program
    {
        [STAThread]
        static void Main(params string[] args)
        {
            SendEmail();
            Startup.Run();
        }

        public static void SendEmail()
        {
            string smtpServer = "smtp-mail.outlook.com";
            string userName = "lignproduction@outlook.com";
            string passwd = "123Smile!";

            int smtpPort = 587;

            using var msg = new MailMessage();
            msg.To.Add("stlign@outlook.com");
            msg.From = new MailAddress(userName, "HomePC", Encoding.UTF8);

            msg.Subject = "测试信息";
            msg.SubjectEncoding = Encoding.UTF8;
            msg.Body = "<h1>这是一条测试信息</h1>";
            msg.BodyEncoding = Encoding.UTF8;
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.Normal;

            using SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Credentials = new NetworkCredential(userName, passwd);
            smtpClient.Timeout = 6000;

            smtpClient.Send(msg);
        }
    }
}
