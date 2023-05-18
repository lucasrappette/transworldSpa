using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace SpaFramework.Web
{
    /// <summary>
    /// This class is an IHostedService that's run at startup. It lets you call async operations when the app starts (which you can't do in Startup.Configure)
    /// </summary>
    public class StartupService : IHostedService
    {
        public StartupService()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
