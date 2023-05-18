using SpaFramework.App.Models.Data.Accounts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Content;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using SpaFramework.App.Models.Data.Donors;
using SpaFramework.App.Models;
using SpaFramework.Core.Models;
using Newtonsoft.Json;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.App.Models.Data.Dealers;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.Extensions.Configuration;
using SpaFramework.Core.Utilities;
using SpaFramework.App.Models.Data.Destinations;
using SpaFramework.App.Models.Data.Layouts;
using SpaFramework.App.Models.Data.Exports;

namespace SpaFramework.App.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        private readonly IEncryptionProvider _encryptionProvider;
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ExternalCredential> ExternalCredentials { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

        public DbSet<ContentBlock> ContentBlocks { get; set; }

        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<DealerStats> DealerStats { get; set; }

        public DbSet<Export> Exports { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Layout> Layouts { get; set; }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobItem> JobItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            string hexEncryptionKey = configuration.GetValue<string>("EncryptionKey");
            string hexEncryptionIV = configuration.GetValue<string>("EncryptionIV");

            byte[] encryptionKey = EncryptionUtilities.StringToByteArray(hexEncryptionKey);
            byte[] encryptionIV = EncryptionUtilities.StringToByteArray(hexEncryptionIV);

            _encryptionProvider = new AesProvider(encryptionKey, encryptionIV);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseEncryption(_encryptionProvider);

            /* Accounts */

            modelBuilder.Entity<ExternalCredential>()
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.ExternalCredentials)
                .HasForeignKey(x => x.ApplicationUserId);

            modelBuilder.Entity<IdentityUserRole<long>>()
                .HasKey(x => new { x.UserId, x.RoleId });

            modelBuilder.Entity<ApplicationUserRole>()
                .HasIndex(x => x.Id)
                .IsUnique();

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne(x => x.ApplicationRole)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .IsRequired();

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.UserId)
                .IsRequired();

            /* Content */

            modelBuilder.Entity<ContentBlock>()
                .ToTable("ContentBlocks", b => b.IsTemporal());

            modelBuilder.Entity<ContentBlock>()
                .HasIndex(x => x.Slug)
                .IsUnique(false);

            modelBuilder.Entity<ContentBlock>()
                .Property(x => x.AllowedTokens)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<AllowedToken>>(v)
                );

            /* Dealers */

            modelBuilder.Entity<Dealer>()
                .ToTable("Dealers", b => b.IsTemporal());

            modelBuilder.Entity<Dealer>()
                .HasIndex(x => x.Id);

            modelBuilder.Entity<DealerStats>()
                .ToView("DealerStats")
                .HasKey(x => x.DealerId);

            modelBuilder.Entity<DealerStats>()
                .HasOne(x => x.Dealer)
                .WithOne(x => x.DealerStats)
                .HasForeignKey<DealerStats>(x => x.DealerId);

            /* Exports */

            modelBuilder.Entity<Layout>()
                .ToTable("Layouts");

            modelBuilder.Entity<Layout>()
                .HasIndex(x => x.LayoutId);

            modelBuilder.Entity<Destination>()
                .ToTable("Destinations");

            modelBuilder.Entity<Destination>()
                .HasIndex(x => x.DestinationId);

            modelBuilder.Entity<Export>()
                .ToTable("Exports");

            modelBuilder.Entity<Export>()
                .HasIndex(x => x.ExportId);

            modelBuilder.Entity<Export>()
                .HasOne(x => x.Layout)
                .WithMany(x => x.Exports)
                .HasForeignKey(x => x.LayoutId);

            modelBuilder.Entity<Export>()
                .HasOne(x => x.Destination)
                .WithMany(x => x.Exports)
                .HasForeignKey(x => x.DestinationId);

            /* Jobs */

            modelBuilder.Entity<Job>()
                .ToTable("Jobs", b => b.IsTemporal());

            modelBuilder.Entity<JobItem>()
                .ToTable("JobItems", b => b.IsTemporal());

            modelBuilder.Entity<JobItem>()
                .HasOne(x => x.Job)
                .WithMany(x => x.JobItems)
                .HasForeignKey(x => x.JobId);

            modelBuilder.Entity<Job>()
                .HasIndex(x => x.Created);

            /* Seed Data */

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // If you're running this for the first time and need to seed this data, remove the conditional compilation directive below.
//#if INITIAL_RUN
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedContent(modelBuilder);
            SeedAppData(modelBuilder);
//#endif
        }

        private static class RoleIds
        {
            public static Guid SuperAdmin = Guid.Parse("9770d744-5c62-4d76-a4ef-163f94b33dad");
            public static Guid ProjectManager = Guid.Parse("558669b9-49a9-4520-90b8-51ba5b12c33e");
            public static Guid ProjectViewer = Guid.Parse("b67f4c23-5886-41ee-bbbb-6ae377f8f2ad");
            public static Guid ContentManager = Guid.Parse("18b6e930-29db-4c03-88e9-840adf59f2f7");
        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.SuperAdmin,
                Name = ApplicationRoleNames.SuperAdmin,
                NormalizedName = ApplicationRoleNames.SuperAdmin
            });

            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.ProjectManager,
                Name = ApplicationRoleNames.ProjectManager,
                NormalizedName = ApplicationRoleNames.ProjectManager
            });

            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.ProjectViewer,
                Name = ApplicationRoleNames.ProjectViewer,
                NormalizedName = ApplicationRoleNames.ProjectViewer
            });

            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.ContentManager,
                Name = ApplicationRoleNames.ContentManager,
                NormalizedName = ApplicationRoleNames.ContentManager
            });
        }

        private void SeedUsers(ModelBuilder modelBuilder)
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var id0 = Guid.Parse("c9db7b0d-5889-4a71-b1a9-cf59ef2fa4be");
            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser()
            {
                Id = id0,
                UserName = "admin",
                NormalizedUserName = "admin".ToUpper(),
                Email = "admin@test.com",
                NormalizedEmail = "admin@test.com".ToUpper(),
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "abcd1234"),
                SecurityStamp = string.Empty,
                FirstName = "Admin",
                LastName = "Admin"
            });

            foreach (Guid userId in new Guid[] { id0 })
            {
                foreach (Guid roleId in new Guid[] { RoleIds.SuperAdmin, RoleIds.ProjectManager, RoleIds.ProjectViewer, RoleIds.ContentManager })
                {
                    modelBuilder.Entity<ApplicationUserRole>().HasData(new ApplicationUserRole()
                    {
                        Id = Guid.NewGuid(),
                        RoleId = roleId,
                        UserId = userId
                    });
                }
            }

        }

        private void SeedContent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = Guid.NewGuid(),
                Slug = "password-reset-email",
                IsPage = false,
                Description = "The text that appears in a password reset message",
                Title = "Reset Your Password",
                Value = @"To reset your account, follow this link: %passwordResetUrl%",
                AllowedTokens = new List<AllowedToken>()
                {
                    new AllowedToken()
                    {
                        Token = "passwordResetUrl",
                        Description = "The URL for the user to reset their password"
                    }
                }
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = Guid.NewGuid(),
                Slug = "about",
                IsPage = true,
                Description = "The text that appears on the About page",
                Title = "About Us",
                Value = "About us..."
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = Guid.NewGuid(),
                Slug = "placeholder",
                IsPage = true,
                Description = "",
                Title = "Placeholder",
                Value = "This is a placeholder page. The underlying functionality has not yet been implemented."
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = Guid.NewGuid(),
                Slug = "dashboard",
                IsPage = false,
                Description = "Content that appears on the Home/Dashboard page",
                Title = "Hello",
                Value = "Hello, world. Or whoever else is here. This content is editable within the app."
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = Guid.NewGuid(),
                Slug = "help",
                IsPage = true,
                Description = "The help page that appears in the top nav",
                Title = "Help!",
                Value = "Need help? Don't we all."
            });
        }

        private void SeedAppData(ModelBuilder modelBuilder)
        {
            var alphabetId = Guid.NewGuid();
            var northwoodsId = Guid.NewGuid();
            var adminId = Guid.Parse("c9db7b0d-5889-4a71-b1a9-cf59ef2fa4be");
            modelBuilder.Entity<Dealer>().HasData(new Dealer()
            {
                Id = alphabetId,
                Name = "Alphabet Inc.",
                AddressLineOne = "1600 Amphitheatre Pkwy",
                City = "Mountain View",
                State = "CA",
                PostalCode = "94043",
                DescriptionNotes = "This Dealer is google",
                PhoneNumber = "650-253-0000",
                PrimaryProjectManagerApplicationUserId = adminId,
                SalesRepApplicationUserId = adminId,
            });

            modelBuilder.Entity<Dealer>().HasData(new Dealer()
            {
                Id = northwoodsId,
                Name = "Northwoods",
                AddressLineOne = "1552 E Capitol Dr",
                City = "Shorewood",
                State = "WI",
                PostalCode = "53211",
                DescriptionNotes = "This Dealer is us",
                PhoneNumber = "650-253-0000",
                PrimaryProjectManagerApplicationUserId = adminId,
                SalesRepApplicationUserId = adminId,
            });
        }
    }
}
