using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpaFramework.App.Models.Data.Accounts
{
    /// <summary>
    /// Roles correspond to a set of permissions. A single user may have multiple roles. The same set of permissions/roles applies to the user across all outlets they have access to
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>, IEntity, IHasId<Guid>
    {
        public Guid GetId() => this.Id;

        [NotMapped]
        public string ConcurrencyCheck
        {
            get { return ConcurrencyStamp; }
            set { ConcurrencyStamp = value; }
        }

        public List<ApplicationUserRole> Users { get; set; }
    }
}
