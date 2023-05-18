using SpaFramework.App.Utilities.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Content
{
    public class ContentBlock : IEntity, IHasId<Guid>, ILoggableName
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

        [NotMapped]
        [ExcludeFromDTO]
        public string LoggableName { get { return Slug; } }

        [MaxLength(100)]
        public string Slug { get; set; }

        public bool IsPage { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public string Value { get; set; }

        public List<AllowedToken> AllowedTokens { get; set; }
    }
}
