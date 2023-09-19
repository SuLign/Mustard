using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.Base.Core
{
    public class MustardService
    {
        public string ServiceFilePath { get; set; }

        internal AppDomain StardService(AppDomainManager domainManager)
        {
            var fileName = Path.GetFileNameWithoutExtension(ServiceFilePath);
            var domain = domainManager.CreateDomain(fileName, new System.Security.Policy.Evidence(), new AppDomainSetup());
            Task.Run(() =>
            {
                domain.ExecuteAssembly(ServiceFilePath);
            });
            return domain;
        }
    }
}
