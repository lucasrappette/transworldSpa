using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SpaFramework.App.Models.BackgroundServices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SpaFramework.Worker.BackgroundServices
{
    public class SingletonBackgroundServicesHost
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SingletonBackgroundServicesHost> _logger;

        public SingletonBackgroundServicesHost(IServiceProvider serviceProvider, ILogger<SingletonBackgroundServicesHost> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(SingletonBackgroundServicesHost.Run))]
        [NoAutomaticTrigger]
        [Singleton]
        public async Task Run(CancellationToken cancellationToken)
        {
            List<Type> backgroundServiceTypes = new List<Type>()
            {
                // Add types here
            };

            List<ISingletonBackgroundService> backgroundServices = new List<ISingletonBackgroundService>();

            foreach (Type type in backgroundServiceTypes)
            {
                ISingletonBackgroundService backgroundService = (ISingletonBackgroundService)_serviceProvider.GetService(type);

                try
                {
                    await backgroundService.Start(cancellationToken);
                    backgroundServices.Add(backgroundService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error starting background service: {backgroundService.GetType().ToString()}");
                }
            }

            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(100);

            foreach (ISingletonBackgroundService backgroundService in backgroundServices)
            {
                try
                {
                    await backgroundService.Stop();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error stopping background service: {backgroundService.GetType().ToString()}");
                }
            }
        }
    }
}
