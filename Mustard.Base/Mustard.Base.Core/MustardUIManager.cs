using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.BaseDefinitions;
using Mustard.Interfaces.Framework;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Mustard.Base.Toolset;

internal class MustardUIManager : IMustardUIManager
{
    private Classes uiClasses;
    private static string _uiExtension = "mustardUI";
    private ConcurrentDictionary<string, Type> mainWindowTypes = new ConcurrentDictionary<string, Type>();
    private ConcurrentDictionary<string, Type> windowTypes = new ConcurrentDictionary<string, Type>();
    private ConcurrentDictionary<string, Window> createdWindows = new ConcurrentDictionary<string, Window>();

    public bool CloseWindow(string windowName)
    {
        throw new NotImplementedException();
    }

    public Window GetEntryWindow(string windowName)
    {
        if (windowName == null) windowName = "mainMustardUI";
        if (mainWindowTypes.ContainsKey(windowName))
        {
            var window = (Window)Activator.CreateInstance(mainWindowTypes[windowName]);
            window.Closed += (_, _) =>
            {
                createdWindows.TryRemove(windowName, out _);
                Debug.WriteLine($"Window {windowName} Unload.");
            };
            return window;
        }
        return null;
    }

    public Window GetWindow(string windowName)
    {
        if (windowTypes.ContainsKey(windowName))
        {
            if (createdWindows.ContainsKey(windowName))
            {
                Debug.WriteLine($"Contained Window Returned.");
                return createdWindows[windowName];
            }
            var window = (Window)Activator.CreateInstance(windowTypes[windowName]);
            createdWindows.TryAdd(windowName, window);
            window.Closed += (_, _) =>
            {
                createdWindows.TryRemove(windowName, out _);
                Debug.WriteLine($"Window {windowName} Unload.");
            };
            return window;
        }
        return null;
    }

    public bool Initialize()
    {
        if (Classes.LoadTypesFromCurrentAssembly<Window, MustardWindowAttribute>(out var windowsInAssembly))
        {
            for (int i = 0; i < windowsInAssembly.Count; i++)
            {
                var (attr, uiWindow) = windowsInAssembly[i];
                RegistWindow(attr.Title??"mainMustardUI", uiWindow, attr.IsEntryPoint);
            }
        }
        var uiLibDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UIs");
        if (!Directory.Exists(uiLibDirectory))
            Directory.CreateDirectory(uiLibDirectory);
        var uiLibFileInAppPath = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll")
            .ToList().FindAll(e => e.ToLower().Contains($".{_uiExtension.ToLower()}."));
        if (uiLibFileInAppPath != null && uiLibFileInAppPath.Count > 0)
        {
            foreach (var item in uiLibFileInAppPath)
            {
                try
                {
                    if (File.Exists(Path.Combine(uiLibDirectory, Path.GetFileName(item))))
                        File.Delete(Path.Combine(uiLibDirectory, Path.GetFileName(item)));
                    File.Move(item, Path.Combine(uiLibDirectory, Path.GetFileNameWithoutExtension(item)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
        var uiLibFiles = Directory.GetFiles(uiLibDirectory, "*.dll", SearchOption.AllDirectories);
        if (uiLibFiles != null && uiLibFiles.Length > 0)
        {
            foreach (var item in uiLibFiles)
            {
                if (Classes.LoadMultiTypeFromDll<MustardWindowAttribute>(item, out var windowsInPlugins, out _))
                {
                    for (int i = 0; i < windowsInPlugins.Count; i++)
                    {
                        var (attr, uiWindow) = windowsInPlugins[i];
                        RegistWindow(attr.Title, uiWindow, attr.IsEntryPoint);
                    }
                }
            }
        }
        return true;
    }

    public TResult RegistWindow(string windowName, Type type, bool isEntryWindow = false)
    {
        if (isEntryWindow)
        {
            if (mainWindowTypes.ContainsKey(windowName))
            {
                Debug.WriteLine($"命名为：[{windowName}]的界面已添加。");
                return false;
            }
            mainWindowTypes.TryAdd(windowName, type);
            Debug.WriteLine($"界面：[{windowName}]已添加");
        }
        else
        {
            if (windowTypes.ContainsKey(windowName))
            {
                Debug.WriteLine($"命名为：[{windowName}]的界面已添加。");
                return false;
            }
            windowTypes.TryAdd(windowName, type);
            Debug.WriteLine($"界面：[{windowName}]已添加");
        }
        return true;
    }
}
