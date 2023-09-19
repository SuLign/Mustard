using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Interfaces.Framework
{
    /// <summary>
    /// 服务：框架系统启动后，自动加载并在后台进行的任务。
    /// </summary>
    public interface IMustardService
    {
        void Load();
        void Unload();
    }
}
