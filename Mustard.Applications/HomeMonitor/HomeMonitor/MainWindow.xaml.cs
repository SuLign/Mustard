using Mustard.Base.Attributes.UIAttributes;
using Mustard.UI.Sunflower;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mustard.UI.MVVM;
using Mustard.UI.Sunflower.ExControls;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Microsoft.Win32;
using System.Reflection;

namespace HomeMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [MustardWindow(true)]
    public partial class MainWindow : SunFlowerWindow
    {
        private string currentUserName;
        private string currentPasswd;
        private Task monitorTask;
        private string currentIPv6;
        private string currentIPv4;
        private bool isRunning;

        public MainWindow()
        {
            SetStartup();
            InitializeComponent();
            SingletonContainer<IMustardMessageManager>.Instance.LogEvent += Instance_LogEvent;
            ContentRendered += MainWindow_ContentRendered;
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.EmailAddress) || string.IsNullOrEmpty(Properties.Settings.Default.Passwd))
            {
                emailCount.Text = Properties.Settings.Default.EmailAddress ?? "";
                pswdBox.Password = Properties.Settings.Default.Passwd ?? "";
            }
            else
            {
                emailCount.Text = Properties.Settings.Default.EmailAddress ?? "";
                pswdBox.Password = Properties.Settings.Default.Passwd ?? "";
                StartMonitor(null, null);
            }
            currentIPv4 = Properties.Settings.Default.CurrentIPv4;
            currentIPv6 = Properties.Settings.Default.CurrentIPv6;
        }

        private void Instance_LogEvent(DateTime arg1, string arg2, Mustard.Base.BaseDefinitions.TResult arg3)
        {
            Dispatcher?.Invoke(() =>
            {
                string msg = arg3;
                var textBlock = new TextBlock()
                {
                    Text = $"{arg1:yyyy/MM/dd HH:mm:ss}\t{msg}"
                };
                logStack.Children.Add(textBlock);
                logScollViewer.ScrollToEnd();
            });
        }

        private void StartMonitor(object sender, RoutedEventArgs e)
        {
            isRunning = true;
            if (string.IsNullOrEmpty(emailCount.Text) || string.IsNullOrWhiteSpace(emailCount.Text))
            {
                MustardMessageBox.Show("请输入邮箱账号");
                return;
            }
            var regRes = Regex.Match(emailCount.Text, @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")$");
            if (!regRes.Success)
            {
                MustardMessageBox.Show("请输入正确的邮箱账号");
                return;
            }
            if (pswdBox.Password.Length == 0)
            {
                MustardMessageBox.Show("请输入邮箱密码");
                return;
            }
            StartServer(emailCount.Text, pswdBox.Password);
        }

        private void StopMonitor(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            if (!monitorTask.IsCompleted)
            {
                monitorTask.Wait();
            }
            btnStartOrStop.Content = "开始监护";
            btnStartOrStop.Click -= StopMonitor;
            btnStartOrStop.Click += StartMonitor;
            SingletonContainer<IMustardMessageManager>.Instance.Log($"停止监护");
        }

        public void StartServer(string username, string passwd)
        {
            currentUserName = username;
            currentPasswd = passwd;
            Properties.Settings.Default.EmailAddress = username;
            Properties.Settings.Default.Passwd = passwd;
            Properties.Settings.Default.Save();
            monitorTask = Task.Run(async () =>
            {
                while (isRunning)
                {
                    try
                    {
                        var addrs = Dns.GetHostAddresses(Dns.GetHostName());
                        var ipv6Addrs = addrs.Where(e =>
                            e.AddressFamily == AddressFamily.InterNetworkV6 &&
                            !e.IsIPv4MappedToIPv6 &&
                            !e.IsIPv6LinkLocal &&
                            !e.IsIPv6Multicast &&
                            !e.IsIPv6SiteLocal).ToList();
                        var ipv4 = GetWANIP();
                        if (currentIPv6 != ipv6Addrs[0].ToString() || currentIPv4 != ipv4)
                        {
                            int faildCount = 0;
                            while (!SendEmail(ipv4, ipv6Addrs[0].ToString()))
                            {
                                faildCount++;
                                if (faildCount == 3)
                                {
                                    StopMonitor(null, null);
                                    return;
                                }
                            }
                        }
                    }
                    finally
                    {
                        await Task.Delay(5000);
                    }
                }
            });
            btnStartOrStop.Content = "停止监护";
            btnStartOrStop.Click -= StartMonitor;
            btnStartOrStop.Click += StopMonitor;
            SingletonContainer<IMustardMessageManager>.Instance.Log($"开始监护");
        }

        public bool SendEmail(string ipv4, string ipv6)
        {
            currentIPv6 = ipv6;
            currentIPv4 = ipv4;
            using var wait = new WaitControl();
            wait.Wait("邮件发送中");
            try
            {
                string smtpServer = "smtp-mail.outlook.com";
                string mailAddress = $"{currentUserName}@outlook.com";

                int smtpPort = 587;

                using var msg = new MailMessage();
                msg.To.Add("stlign@outlook.com");
                msg.From = new MailAddress(mailAddress, "HomePC", Encoding.UTF8);

                msg.Subject = "IP变更";
                msg.SubjectEncoding = Encoding.UTF8;
                msg.Body =
                    $"<b>变更时间:{DateTime.Now:yyyy年MM月dd日 HH:mm:ss}</b>\n" +
                    $"<b>当前本机IP</b>\n" +
                    $"<div>IPv6: {ipv6}</div>\n" +
                    $"<div>IPv4: {ipv4}</div>\n";
                msg.BodyEncoding = Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Priority = MailPriority.Normal;

                using SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential(mailAddress, currentPasswd);
                smtpClient.Timeout = 20000;

                smtpClient.Send(msg);
                SingletonContainer<IMustardMessageManager>.Instance.Log($"IP地址更新，发送邮件成功！");
                Properties.Settings.Default.CurrentIPv4 = ipv4;
                Properties.Settings.Default.CurrentIPv6 = ipv6;
                Properties.Settings.Default.Save();
                return true;
            }
            catch (Exception ex)
            {
                SingletonContainer<IMustardMessageManager>.Instance.Log(ex.Message);
                return false;
            }
        }

        public string GetWANIP()
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

        public void SetStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("HomeMonitor", Assembly.GetExecutingAssembly().Location);
        }
    }
}
