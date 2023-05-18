using SpaFramework.App.Models.Service.WorkItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Service.WorkItems.Echo
{
    /// <summary>
    /// This is just for testing/debugging. It has no functional purposes -- it's essentially a "Hello, World" mechanism for testing the work item framework.
    /// </summary>
    public class EchoWorkItem : IWorkItem
    {
        public string LoggableName => "";

        public string Message { get; set; }
    }
}
