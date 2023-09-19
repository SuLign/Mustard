using Mustard.Base.BaseDefinitions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mustard.Interfaces.Framework
{
    public interface IMustardUIManager
    {
        /// <summary>
        /// 通过窗口名获取窗口（创建）
        /// </summary>
        /// <param name="windowName">窗口名</param>
        /// <returns>窗口</returns>
        Window GetWindow(string windowName);

        /// <summary>
        /// 获取主窗口（入口）
        /// </summary>
        /// <param name="windowName">窗口名</param>
        /// <returns>窗口</returns>
        Window GetEntryWindow(string windowName = null);

        bool Initialize();

        /// <summary>
        /// 注册UI界面
        /// </summary>
        /// <param name="windowName">UI界面名</param>
        /// <typeparam name="type">UI界面类型</typeparam>
        /// <param name="isEntryWindow">是否为主界面（入口界面）</param>
        /// <returns>注册结果</returns>
        TResult RegistWindow(string windowName, Type type, bool isEntryWindow = false);

        /// <summary>
        /// 关闭已打开窗口
        /// </summary>
        /// <param name="windowName">UI界面名</param>
        /// <returns>执行结果</returns>
        bool CloseWindow(string windowName);
    }
}
