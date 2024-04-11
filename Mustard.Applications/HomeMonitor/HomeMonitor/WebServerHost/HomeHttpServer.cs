using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HomeMonitor.WebServerHost;

internal class HomeHttpServer
{
    private HttpListener listener;

    public HomeHttpServer()
    {
        listener = new HttpListener();

    }

    public void StartListen()
    {
        var ipList = Dns.GetHostAddresses(Dns.GetHostName());
        foreach (var ip in ipList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                listener.Prefixes.Add(ip.ToString());
            }
        }
        listener.Start();


    }

    private void Monitor()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var context = listener.GetContext();
            }
        });
    }
}
