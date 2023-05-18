using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Service.WorkItems
{
    public class WorkItemMessage<T>
        where T : IWorkItem
    {
        private static Random _random = new Random();

        public T WorkItem { get; set; }
        public int Attempts { get; set; }

        public int? InitialDelaySeconds { get; set; }
        public int? RequeueDelaySeconds { get; set; }
        public TimeSpan TimeToLive { get; set; }

        [JsonIgnore]
        public bool CanRequeue
        {
#if DEBUG
            get { return Attempts < 2; }
#else
            get { return Attempts < 20; }
#endif
        }

        public TimeSpan GetInitialVisibilityDelay()
        {
            if (Attempts == 0 && InitialDelaySeconds.HasValue)
                return TimeSpan.FromSeconds(InitialDelaySeconds.Value);
            else if (Attempts > 0 && Attempts < 20 && RequeueDelaySeconds.HasValue)
                return TimeSpan.FromSeconds(RequeueDelaySeconds.Value);

            if (Attempts == 0)
                return TimeSpan.FromSeconds(1);
            else if (Attempts == 1)
                return TimeSpan.FromSeconds(5);
            else if (Attempts < 4)
            {
                int jitter = _random.Next(10);
                return TimeSpan.FromSeconds(10 + jitter);
            }
            else if (Attempts < 10)
            {
                int jitter = _random.Next(60);
                return TimeSpan.FromSeconds(60 + jitter);
            }
            else if (Attempts < 20)
            {
                int jitter = _random.Next(300);
                return TimeSpan.FromSeconds(60 + jitter);
            }
            else
                throw new Exception("Too many requeue attempts");
        }
    }
}
