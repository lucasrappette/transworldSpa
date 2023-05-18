using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaFramework.App.Models.Data.Exports;

namespace SpaFramework.App.Models.Data.Destinations
{
    public class Destination : IEntity, IHasId<int>, ILoggableName
    {
        public int GetId() => DestinationId;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DestinationId { get; set; }

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
        public string LoggableName { get { return DestinationId.ToString(); } }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string FtpHost { get; set; }

        [MaxLength(50)]
        [Required]
        public string FtpUsername { get; set; }

        [MaxLength(50)]
        [Required]
        public string FtpPassword { get; set; }

        [MaxLength(50)]
        public string FtpRemoteDir { get; set; }




        public List<Export> Exports { get; set; }
    }
}
