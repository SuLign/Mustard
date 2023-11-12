using Mustard.Base.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGTool
{
    public class Program
    {
        [STAThread]
        static void Main(params string[] args)
        {
            Startup.Run();
        }
    }
}
