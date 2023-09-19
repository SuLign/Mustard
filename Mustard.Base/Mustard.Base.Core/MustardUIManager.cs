using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.BaseDefinitions;
using Mustard.Interfaces.Framework;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Mustard.Base.Toolset;

internal class MustardUIManager : IMustardUIManager
{
    private Classes uiClasses;
    public bool CloseWindow(string windowName)
    {
        throw new NotImplementedException();
    }

    public Window GetEntryWindow(string windowName)
    {
        var entryWindows =
            uiClasses.BasedOn<Window, MustardWindowAttribute>
            (e => windowName == null ? windowName == e.Title && e.IsEntryPoint : e.IsEntryPoint);
        if (entryWindows == null || entryWindows.Length == 0) 
            throw new FileNotFoundException("未找到程序入口页面");
        if(entryWindows.Length != 1) 
            throw new FileNotFoundException("找到多个程序入口页面");
        var window = (Window)Activator.CreateInstance(entryWindows[0]);
        return window;
    }

    public Window GetWindow(string windowName)
    {
        throw new NotImplementedException();
    }

    public bool Initialize()
    {
        uiClasses = Classes.FromEntryPointAssembly();
        var uiLibDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UIs");
        if (!Directory.Exists(uiLibDirectory))
            Directory.CreateDirectory(uiLibDirectory);
        var uiLibFileInAppPath = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll")
            .ToList().FindAll(e => e.ToLower().Contains(".jwui."));
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
                uiClasses.AddAssemblyFromFile(item);
            }
        }
        return true;
    }

    public TResult RegistWindow(string windowName, Type type, bool isEntryWindow = false)
    {
        throw new NotImplementedException();
    }
}
