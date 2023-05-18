using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Dealers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaFramework.App.Models.Data.Layouts;
using SpaFramework.App.Models.Data.Destinations;

namespace SpaFramework.App.Models.Data.Exports
{
    public class Export : IEntity, IHasId<int>, ILoggableName
    {
        public int GetId() => ExportId;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExportId { get; set; }

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
        public string LoggableName { get { return ExportId.ToString(); } }

        public string ExportName { get; set; }
        public DateTime RunTimeOne { get; set; }
        public DateTime RunTimeTwo { get; set; }


        public int LayoutId { get; set; }
        public Layout Layout { get; set; }

        public int DestinationId { get; set; }
        public Destination Destination { get; set; }

    }
}

