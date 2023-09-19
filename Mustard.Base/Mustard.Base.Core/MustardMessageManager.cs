using Mustard.Base.BaseDefinitions;
using Mustard.Interfaces.Framework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mustard.Base.Core;

internal class MustardMessageManager : IMustardMessageManager
{
    private sealed record ErrorRecord(TResult err, string callerFile, int callerLineNumber, string callerMemberName);
    private sealed record LogRecord(TResult err, string callerMemberName);

    private bool isErrQueueInLoop;
    private bool isLogQueueInLoop;
    private string errorLogPath;
    private string logPath;

    private ConcurrentQueue<ErrorRecord> errorRecords;
    private ConcurrentQueue<LogRecord> logRecords;
    private bool outputErrorCaller;
    private bool outputErrorLine;
    private bool outputErrorFile;

    public bool OutputErrorCaller { get => outputErrorCaller; set => outputErrorCaller = value; }
    public bool OutputErrorLine { get => outputErrorLine; set => outputErrorLine = value; }
    public bool OutputErrorFile { get => outputErrorFile; set => outputErrorFile = value; }

    public event Action<string, int, string, TResult> ErrorEvent;
    public event Action<string, TResult> NoticeEvent;
    public event Action<string, TResult> MessageBoxEvent;
    public event Action<DateTime, string, TResult> LogEvent;

    public void Error
        (TResult err,
        [CallerFilePath] string callerFile = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = null)
    {
        var errorRecord = new ErrorRecord(err, callerFile, callerLineNumber, callerMemberName);
        errorRecords.Enqueue(errorRecord);
        ErrorEvent?.Invoke(callerFile, callerLineNumber, callerMemberName, err);
        if (!isErrQueueInLoop)
        {
            isErrQueueInLoop = true;
            Task.Run(async () =>
            {
                int emptyTimes = 0;
                using (var errLogWriter = new StreamWriter(errorLogPath, true))
                    while (true)
                    {
                        await Task.Delay(500);
                        if (errorRecords.TryDequeue(out var errorRecord))
                        {
                            var (errMsg, errFile, errLineNumber, errMemberName) = errorRecord;
                            var errorLogMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}\t\"{errMsg}\"";
#if DEBUG
                            if (OutputErrorCaller) errorLogMessage = errorLogMessage + $", <{errMemberName}>";
                            if (OutputErrorLine) errorLogMessage = errorLogMessage + $", Line [{errLineNumber}]";
                            if (OutputErrorFile) errorLogMessage = errorLogMessage + $", {errFile}";
                            Debug.WriteLine(errorLogMessage);
#endif
                            errLogWriter.WriteLine(errorLogMessage);
                            emptyTimes = 0;
                            continue;
                        }
                        emptyTimes++;
                        if (emptyTimes > 4) break;
                    }
                isErrQueueInLoop = false;
            });
        }
    }

    public void Log(TResult text, [CallerMemberName] string callerMemberName = null)
    {
        var logRecord = new LogRecord(text, callerMemberName);
        logRecords.Enqueue(logRecord);
        LogEvent?.Invoke(DateTime.Now, callerMemberName, text);
        if (!isLogQueueInLoop)
        {
            isLogQueueInLoop = true;
            Task.Run(async () =>
            {
                int emptyTimes = 0;
                using (var logWriter = new StreamWriter(logPath, true))
                    while (true)
                    {
                        await Task.Delay(500);
                        if (logRecords.TryDequeue(out var logRecord))
                        {
                            var (logMsg, caller) = logRecord;
                            var logMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}\t\"{logMsg}\"";
#if DEBUG
                            logMessage = logMessage + $", <{caller}>";
                            Debug.WriteLine(logMessage);
#endif
                            logWriter.WriteLine(logMessage);
                            emptyTimes = 0;
                            continue;
                        }
                        emptyTimes++;
                        if (emptyTimes > 4) break;
                    }
                isLogQueueInLoop = false;
            });
        }
    }

    public void MessageBoxShow(TResult text, MessageShowType showType, [CallerMemberName] string callerMemberName = null)
    {
    }

    public void Notice(TResult notice, [CallerMemberName] string callerMemberName = null)
    {
    }

    public MustardMessageManager()
    {
        var envPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppDomain.CurrentDomain.FriendlyName);
        if (!Directory.Exists(envPath))
        {
            Directory.CreateDirectory(envPath);
        }
        errorLogPath = Path.Combine(envPath, $"Err{DateTime.Now:yyyyMMdd}.log");
        logPath = Path.Combine(envPath, $"log{DateTime.Now:yyyyMMdd}.log");
        errorRecords = new ConcurrentQueue<ErrorRecord>();
        logRecords = new ConcurrentQueue<LogRecord>();
    }
}
