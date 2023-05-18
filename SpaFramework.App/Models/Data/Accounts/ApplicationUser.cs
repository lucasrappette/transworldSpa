using Microsoft.AspNetCore.Identity;
using SpaFramework.App.Utilities.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaFramework.App.Models.Data.Accounts
{
    [ExcludeFromDTO(Properties = new string[] { "PasswordHash" })]

    public class ApplicationUser : IdentityUser<Guid>, IEntity, IHasId<Guid>
    {
        public Guid GetId() => this.Id;

        [NotMapped]
        public string ConcurrencyCheck
        {
            get { return ConcurrencyStamp; }
            set { ConcurrencyStamp = value; }
        }

        public List<ApplicationUserRole> Roles { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public List<ExternalCredential> ExternalCredentials { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get { return FirstName + " " + LastName; }
        }

        [NotMapped]
        public bool HasPassword
        {
            get { return !string.IsNullOrEmpty(PasswordHash); }
        }
    }
}
