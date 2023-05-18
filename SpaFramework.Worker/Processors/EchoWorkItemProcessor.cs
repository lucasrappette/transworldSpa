using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SpaFramework.App.Models.Service.WorkItems;
using SpaFramework.App.Models.Service.WorkItems.Echo;
using SpaFramework.App.Services.WorkItems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.Worker.WorkItemProcessors
{
    public class EchoWorkItemProcessor
    {
        private readonly ILogger<EchoWorkItemProcessor> _logger;
        private readonly IWorkItemService<EchoWorkItem> _workItemService;

        public EchoWorkItemProcessor(ILogger<EchoWorkItemProcessor> logger, IWorkItemService<EchoWorkItem> workItemService)
        {
            _logger = logger;
            _workItemService = workItemService;
        }

        public async Task ProcessMessage([QueueTrigger("echoworkitem")] string message)
        {
            var workItemMessage = JsonConvert.DeserializeObject<WorkItemMessage<EchoWorkItem>>(message);
            try
            {
                _logger.LogInformation("Echo Message: {EchoMessage}", workItemMessage.WorkItem.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing EchoWorkItem");

                await _workItemService.EnqueueWorkItem(workItemMessage);
            }
        }
    }
}
