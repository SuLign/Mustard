using Mustard.Interfaces.Framework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Base.Core
{
    internal class MustardPluginManager : 
        IMustardPluginManager
    {
        private ConcurrentDictionary<Type, List<string>> allPluginTypes;
        private ConcurrentDictionary<Type, ConcurrentDictionary<string, IMustardPlugin>> loadedPlugins;
        public T GetPlugin<T>(string pluginName) where T : IMustardPlugin
        {
            throw new NotImplementedException();
        }

        public T GetPlugin<T>() where T : IMustardPlugin
        {
            throw new NotImplementedException();
        }

        public List<Tuple<string, IMustardPlugin>> GetLoadedPluginList()
        {
            throw new NotImplementedException();
        }

        public List<string> GetPluginList<T>() where T : IMustardPlugin
        {
            var typeofT = typeof(T);
            if (allPluginTypes.ContainsKey(typeofT))
            {
                return allPluginTypes[typeofT];
            }
            return default;
        }

        public bool SetDefaultPlugin<T>(string pluginName) where T : IMustardPlugin
        {
            throw new NotImplementedException();
        }

        public bool SetDefaultPlugin<T>(T plugin) where T : IMustardPlugin
        {
            throw new NotImplementedException();
        }

        public bool SetLoadPluginList(List<string> pluginFileNames)
        {
            throw new NotImplementedException();
        }

        public MustardPluginManager()
        {
            allPluginTypes = new ConcurrentDictionary<Type, List<string>>();
            loadedPlugins = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IMustardPlugin>>();
        }
    }
}
