using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Base.BaseDefinitions.Exceptions;

public class MustardServiceNullException:Exception
{
    public MustardServiceNullException() { }
    public MustardServiceNullException(string message) : base(message) { }
}
