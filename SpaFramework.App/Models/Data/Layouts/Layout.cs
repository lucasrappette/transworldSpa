using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Dealers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaFramework.App.Models.Data.Exports;

namespace SpaFramework.App.Models.Data.Layouts
{
    public  class Layout : IEntity, IHasId<int>, ILoggableName
    {
        public int GetId() => LayoutId;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LayoutId { get; set; }

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
        public string LoggableName { get { return LayoutId.ToString(); } }

        public List<Export> Exports { get; set; }
    }
}
