using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Interfaces.Framework
{
    public interface IMustardPluginManager
    {
        /// <summary>
        /// 获取指定名称插件
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <param name="pluginName">插件名</param>
        /// <returns>插件</returns>
        T GetPlugin<T>(string pluginName) where T : IMustardPlugin;

        /// <summary>
        /// 获取该类型的默认插件。
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <returns>插件</returns>
        T GetPlugin<T>() where T : IMustardPlugin;

        /// <summary>
        /// 获取已加载插件列表
        /// </summary>
        /// <returns>插件列表</returns>
        List<Tuple<string, IMustardPlugin>> GetLoadedPluginList();

        /// <summary>
        /// 获取指定类型的插件列表
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <returns>插件列表</returns>
        List<string> GetPluginList<T>() where T : IMustardPlugin;

        /// <summary>
        /// 设置插件为该类型插件的默认使用插件
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <param name="pluginName">插件名</param>
        /// <returns>执行结果</returns>
        bool SetDefaultPlugin<T>(string pluginName) where T : IMustardPlugin;

        /// <summary>
        /// 设置插件为该类型插件的默认使用插件
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <param name="plugin">插件</param>
        /// <returns>执行结果</returns>
        bool SetDefaultPlugin<T>(T plugin)where T : IMustardPlugin;

        /// <summary>
        /// 设置加载插件列表
        /// </summary>
        /// <param name="pluginFileNames">插件文件路径</param>
        /// <returns>执行结果</returns>
        bool SetLoadPluginList(List<string> pluginFileNames);
    }
}
