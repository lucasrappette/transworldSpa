using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.BackgroundServices
{
    public interface ISingletonBackgroundService
    {
        Task Start(CancellationToken cancellationToken);
        Task Stop();
    }
}
