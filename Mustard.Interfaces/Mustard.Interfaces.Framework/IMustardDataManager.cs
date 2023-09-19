using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Interfaces.Framework;

public interface IMustardDataManager
{
    void PushDataIn<T>(T data);
    void SubscribleData<T>(Action<T> action, Guid id);
    void Unsubcriable<T>(Guid id);
}
