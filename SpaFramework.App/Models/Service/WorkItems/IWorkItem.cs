using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Service.WorkItems
{
    public interface IWorkItem
    {
        string LoggableName { get; }
    }
}
