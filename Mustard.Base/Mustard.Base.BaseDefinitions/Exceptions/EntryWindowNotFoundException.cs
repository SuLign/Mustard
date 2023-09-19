using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Base.BaseDefinitions.Exceptions;

public class EntryWindowNotFoundException:Exception
{
    public EntryWindowNotFoundException() { }
    public EntryWindowNotFoundException(string message) : base(message) { }
}
