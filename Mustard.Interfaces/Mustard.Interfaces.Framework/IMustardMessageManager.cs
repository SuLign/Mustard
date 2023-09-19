using Mustard.Base.BaseDefinitions;

using System;
using System.Runtime.CompilerServices;

namespace Mustard.Interfaces.Framework;

public interface IMustardMessageManager
{
    public event Action<string, int, string, TResult> ErrorEvent;
    public event Action<string, TResult> NoticeEvent;
    public event Action<string, TResult> MessageBoxEvent;
    public event Action<DateTime, string, TResult> LogEvent;

    bool OutputErrorCaller { get; set; }
    bool OutputErrorLine { get; set; }
    bool OutputErrorFile { get; set; }

    /// <summary>
    /// 推送错误提示
    /// </summary>
    /// <param name="err">错误内容</param>
    /// <param name="callerFile">出错文件路径（无需填值，反射自动填充）</param>
    /// <param name="callerLineNumber">出错源码行号（无需填值，反射自动填充）</param>
    /// <param name="callerMemberName">调用方名（无需填值，反射自动填充）</param>
    void Error(
        TResult err,
        [CallerFilePath] string callerFile = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = null);

    /// <summary>
    /// 推送提示信息
    /// </summary>
    /// <param name="notice">提示信息</param>
    /// <param name="callerMemberName">调用方名（无需填值，反射自动填充）</param>
    void Notice(
        TResult notice,
        [CallerMemberName] string callerMemberName = null);

    /// <summary>
    /// 推送消息弹窗信息
    /// </summary>
    /// <param name="text">弹窗信息</param>
    /// <param name="showType">提示消息类型</param>
    /// <param name="callerMemberName">调用方名（无需填值，反射自动填充）</param>
    void MessageBoxShow(
        TResult text,
        MessageShowType showType,
        [CallerMemberName] string callerMemberName = null);

    /// <summary>
    /// 推送日志信息
    /// </summary>
    /// <param name="text">日志信息</param>
    /// <param name="callerMemberName">调用方名（无需填值，反射自动填充）</param>
    void Log(
        TResult text,
        [CallerMemberName] string callerMemberName = null);
}
