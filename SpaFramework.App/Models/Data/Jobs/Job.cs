using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using SpaFramework.App.Utilities.Serialization;

namespace SpaFramework.App.Models.Data.Jobs
{
    /// <summary>
    /// A record of an automated task performed by the system
    /// </summary>
    public class Job : IEntity, IHasId<Guid>, ILoggableName
    {
        public Guid GetId() => this.Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [NotMapped]
        [ExcludeFromDTO]
        public string LoggableName { get { return Name; } }

        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? Ended { get; set; }
        
        public long ExpectedCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }

        [MaxLength(50)]
        public string ItemType { get; set; }

        [NotMapped]
        public List<Guid> ItemIds { get; set; } = new List<Guid>();

        [Column("ItemIds")]
        public string SerializedItemIds
        {
            get { return JsonConvert.SerializeObject(ItemIds); }
            set { ItemIds = string.IsNullOrEmpty(value) ? new List<Guid>() : JsonConvert.DeserializeObject<List<Guid>>(value); }
        }

        public List<JobItem> JobItems { get; set; }
    }
}
