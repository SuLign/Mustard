using Mustard.Base.BaseDefinitions.Exceptions;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Mustard.Base.Core
{
    public class Startup
    {
        public static event LoadCompletedEventHandler ApplicationLoadCompleted;

        public static void Run()
        {
            Initialize();

            Application app = new Application();
            app.LoadCompleted += ApplicationLoadCompleted;
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
        }
    }
}
