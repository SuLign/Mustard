using Mustard.Base.BaseDefinitions.Exceptions;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;

using System;
using System.Windows;

namespace Mustard.Base.Core;

public class Startup
{
    public static event Action AppActived;

    public static void Run()
    {
        Initialize();

        Application app = new();
        var entryWindow = SingletonContainer<IMustardUIManager>.Instance.GetEntryWindow();
        app.Run(entryWindow);
    }

    public static void RunService(MustardService[] services)
    {
        if(services == null)
            throw new MustardServiceNullException();
        Initialize();
        var domainManager = new AppDomainManager();
        foreach (var item in services)
        {
            item.StardService(domainManager);
        }
        Application app = new Application();
        app.Run();
    }

    private static void Initialize()
    {
        SingletonContainer<IMustardUIManager>.RegisterSingletonInstance(new MustardUIManager(), null);
        SingletonContainer<IMustardUIManager>.Instance.Initialize();
        SingletonContainer<IMustardMessageManager>.RegisterSingletonInstance(new MustardMessageManager(), null);
        AppActived?.Invoke();
    }
}
