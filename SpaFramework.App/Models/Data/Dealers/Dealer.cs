using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Dealers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Dealers
{
    /// <summary>
    /// A communications channel through which a donor or schedule can be created or modified
    /// </summary>
    public class Dealer : IEntity, IHasId<Guid>, ILoggableName
    {
        public Guid GetId() => Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Timestamp]
        [ConcurrencyCheck]
        public byte[] ConcurrencyTimestamp { get; set; }

        [NotMapped]
        public string ConcurrencyCheck
        {
            get { return ConcurrencyTimestamp == null ? null : Convert.ToBase64String(ConcurrencyTimestamp); }
            set { ConcurrencyTimestamp = value == null ? null : Convert.FromBase64String(value); }
        }

        [NotMapped]
        public string LoggableName { get { return Name; } }

        [MaxLength(50)]
        public string Name { get; set; }

        public string DescriptionNotes { get; set; }

        public string AddressLineOne { get; set; }
        public string AddressLineTwo { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public decimal Mileage { get; set; }
        public string PhoneNumber { get; set; }
        public string DealerId { get; set; }
        public bool Inactive { get; set; }

        public Guid SalesRepApplicationUserId { get; set; }
        public ApplicationUser SalesRep { get; set; }

        public Guid PrimaryProjectManagerApplicationUserId { get; set; }
        public ApplicationUser PrimaryProjectManager { get; set; }

        public DealerStats DealerStats { get; set; }
    }
}
