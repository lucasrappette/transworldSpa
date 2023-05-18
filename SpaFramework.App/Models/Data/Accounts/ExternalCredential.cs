using SpaFramework.App.Utilities.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Accounts
{
    public class ExternalCredential : IEntity, IHasId<Guid>
    {
        public Guid GetId() => this.Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Timestamp]
        [ConcurrencyCheck]
        [ExcludeFromDTO]
        public byte[] ConcurrencyTimestamp { get; set; }

        [NotMapped]
        public string ConcurrencyCheck
        {
            get { return ConcurrencyTimestamp == null ? null : Convert.ToBase64String(ConcurrencyTimestamp); }
            set { ConcurrencyTimestamp = value == null ? null : Convert.FromBase64String(value); }
        }

        public ApplicationUser ApplicationUser { get; set; }
        public Guid ApplicationUserId { get; set; }

        [MaxLength(20)]
        public string Provider { get; set; }

        [MaxLength(100)]
        public string ExternalId { get; set; }
    }
}
