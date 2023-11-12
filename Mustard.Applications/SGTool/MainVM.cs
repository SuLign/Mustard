using Microsoft.Win32;

using Mustard.UI.MVVM;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SGTool;

internal class MainVM : ViewModelBase
{
    private ObservableCollection<string> localIPv6 = new();
    private string targetIPv6;
    private double sendOrReceiveProcess = 0;
    private string selectedLocalIPv6;
    private Socket recvSocket;
    private string messageNotice;
    private string fileToRead;
    private ObservableCollection<StringLine> contentCollection;

    public string FileToRead
    {
        get => fileToRead;
        set
        {
            fileToRead = value;
            Set();
        }
    }

    public LazyCommand OpenFile => new LazyCommand(() =>
    {
        var fileDialog = new OpenFileDialog();
        if ((bool)fileDialog.ShowDialog())
        {
            FileToRead = fileDialog.FileName;
        }
    });

    public string MessageNotice
    {
        get => messageNotice;
        set
        {
            messageNotice = value;
            Set();
        }
    }

    public LazyCommand ReadFile => new LazyCommand(() =>
    {
        using var fileStream = new FileStream(fileToRead, FileMode.Open, FileAccess.Read);
        //using var bufferedStream = new BufferedStream(fileStream);
        //using var streamReader = new StreamReader(bufferedStream);
        using var streamReader = new StreamReader(fileStream);
        int i = 0;
        ContentCollection.Clear();
        int totalLines = 0;
        //while (streamReader.Peek() != -1)
        //{
        //    _ = streamReader.ReadLine();
        //    totalLines++;
        //}
        fileStream.Position = 100000;
        while (i++ <= 100000)
        {
            var line = streamReader.ReadLine();
            ContentCollection.Add(new StringLine
            {
                ContentText = line,
                LineNumber = i
            });
        }
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

    public ObservableCollection<StringLine> ContentCollection
    {
        get => contentCollection;
        set
        {
            contentCollection = value;
            Set();
        }
    }

    public MainVM()
    {
        ContentCollection = new();
    }
}
