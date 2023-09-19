using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Interfaces.Framework
{
    public interface IMustardServiceManager
    {
        T GetService<T>(string serviceName) where T : IMustardService;

        List<Tuple<string, IMustardService>> GetPluginList();

        /// <summary>
        /// 设置加载服务列表
        /// </summary>
        /// <param name="pluginFileNames">服务文件路径</param>
        /// <returns>执行结果</returns>
        bool SetLoadServiceList(List<string> pluginFileNames);
    }
}
