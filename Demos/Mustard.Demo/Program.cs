using Mustard.Base.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Mustard.Demo
{
    public class Program
    {
        [STAThread]
        static void Main(params string[] args)
        {
            //SendEmail();
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

            var addrs = Dns.GetHostAddresses(Dns.GetHostName());
            var ipv6Addrs = addrs.Where(e =>
                e.AddressFamily == AddressFamily.InterNetworkV6 &&
                !e.IsIPv4MappedToIPv6 &&
                !e.IsIPv6LinkLocal &&
                !e.IsIPv6Multicast &&
                !e.IsIPv6SiteLocal).ToList();
            var ipv4 = GetWANIP();
            msg.Subject = "测试信息";
            msg.SubjectEncoding = Encoding.UTF8;
            msg.Body =
                $"<h1>当前本机IP</h1>\n" +
                $"<div>IPv6: {ipv6Addrs[0]}</div>\n" +
                $"<div>IPv4: {ipv4}</div>\n";
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

        public static string GetWANIP()
        {//创建
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://ip111.cn/");
            //设置请求方法
            httpWebRequest.Method = "GET";
            //请求超时时间
            httpWebRequest.Timeout = 20000;
            //发送请求
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //利用Stream流读取返回数据
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
            //获得最终数据，一般是json
            string responseContent = streamReader.ReadToEnd();

            var res = Regex.Match(responseContent, @"(?<ipAddress>((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}) 中国");
            streamReader.Close();
            httpWebResponse.Close();
            return res.Groups["ipAddress"].ToString();
        }
    }
}
