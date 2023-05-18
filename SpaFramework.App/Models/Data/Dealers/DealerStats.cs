using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Dealers
{
    public class DealerStats : IEntity, IHasId<Guid>
    {
        public Guid GetId() => DealerId;

        public Guid DealerId { get; set; }
        public Dealer Dealer { get; set; }
    }
}
