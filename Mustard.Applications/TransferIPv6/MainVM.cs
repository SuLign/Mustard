using Mustard.UI.MVVM;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TransferIPv6;

internal class MainVM : ViewModelBase
{
    private ObservableCollection<string> localIPv6 = new();
    private string targetIPv6;
    private double sendOrReceiveProcess = .30;
    private string selectedLocalIPv6;
    private Socket recvSocket;
    private string fileToSend;
    private string messageNotice;

    public ObservableCollection<string> LocalIPv6s
    {
        get => localIPv6;
        set
        {
            localIPv6 = value;
            Set();
            GetLocalIPv6();
        }
    }

    public string SelectedLocalIPv6
    {
        get => selectedLocalIPv6;
        set
        {
            selectedLocalIPv6 = value;
            Set();
        }
    }

    public LazyCommand CheckIPv6Valition => new LazyCommand(() =>
    {
        GetLocalIPv6();
    });

    public LazyCommand StartReceive => new LazyCommand(() =>
    {
        StartListenAtIPv6();
    });

    public string TargetIPv6
    {
        get => targetIPv6;
        set
        {
            targetIPv6 = value;
            Set();
        }
    }

    public LazyCommand OpenFile => new LazyCommand(() =>
    {

    });

    public string FileToSend
    {
        get => fileToSend;
        set
        {
            fileToSend = value;
            Set();
        }
    }

    public string MessageNotice
    {
        get => messageNotice;
        set
        {
            messageNotice = value;
            Set();
        }
    }

    public LazyCommand SendFile => new LazyCommand(() =>
    {
        MakeConnection();
    });

    public double SendOrReceiveProcess
    {
        get => sendOrReceiveProcess;
        set
        {
            sendOrReceiveProcess = value;
            Set();
        }
    }

    public MainVM()
    {
        GetLocalIPv6();
    }

    private void GetLocalIPv6()
    {
        var dns = Dns.GetHostName();
        var ipaddres = Dns.GetHostAddresses(dns);
        LocalIPv6s.Clear();
        foreach (var ip in ipaddres)
        {
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                LocalIPv6s.Add(ip.ToString());
            }
        }
        if (LocalIPv6s.Count != 0)
        {
            SelectedLocalIPv6 = LocalIPv6s.FirstOrDefault();
        }
    }

    private void StartListenAtIPv6()
    {
        try
        {
            if (recvSocket != null)
            {
                recvSocket.Close();
            }
            recvSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            recvSocket.Bind(new IPEndPoint(IPAddress.Parse(SelectedLocalIPv6), 20012));
            recvSocket.Listen(20);
            recvSocket.BeginAccept(NewConnection, recvSocket);
            MessageNotice = $"提示：启动接收成功。";
        }
        catch (Exception ex)
        {
            MessageNotice = $"错误提示：{ex.Message}";
        }
    }

    private void NewConnection(IAsyncResult ar)
    {
        try
        {
            var listener = (Socket)ar.AsyncState;
            var clientSocket = listener.EndAccept(ar);
            MustardMessageBox.Show($"新连接：{clientSocket.RemoteEndPoint}。");
            recvSocket.BeginAccept(NewConnection, recvSocket);
        }
        catch { }
    }

    private void MakeConnection()
    {
        try
        {
            if (string.IsNullOrEmpty(TargetIPv6) || string.IsNullOrWhiteSpace(TargetIPv6))
            {
                MessageNotice = $"提示：请输入目标IPv6地址。";
                return;
            }
            var sendSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            sendSocket.Connect(TargetIPv6, 20012);

            MessageNotice = $"提示：连接成功。";
        }
        catch (Exception ex)
        {
            MessageNotice = $"错误提示：{ex.Message}";
        }
    }
}
