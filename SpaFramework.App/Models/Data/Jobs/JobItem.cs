using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpaFramework.App.Models.Data.Jobs
{
    /// <summary>
    /// A record of an automated task performed by the system
    /// </summary>
    public class JobItem : IEntity, IHasId<Guid>
    {
        public Guid GetId() => this.Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid JobId { get; set; }
        public Job Job { get; set; }

        public Guid? ItemId { get; set; }

        public DateTime Timestamp { get; set; }
        
        public string Note { get; set; }
    }
}
