using Mustard.UI.MVVM;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGTool
{
    internal class StringLine : ViewModelBase
    {
        public long LineNumber { get; set; }
        public string ContentText { get; set; }
    }
}
