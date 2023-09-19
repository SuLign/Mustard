using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Interfaces.Framework
{
    /// <summary>
    /// 插件：在框架系统运行过程中，系统或系统组件调用执行的功能模块。
    /// </summary>
    public interface IMustardPlugin
    {
        void Load();

        void Unload();
    }
}
