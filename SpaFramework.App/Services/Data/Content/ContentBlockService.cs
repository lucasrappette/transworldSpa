using FluentValidation;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Content;
using SpaFramework.App.Models.Service.Content;
using MarkdownSharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SpaFramework.App.Models;
using SpaFramework.Core.Models;

namespace SpaFramework.App.Services.Data.Content
{
    public class ContentBlockService : EntityWriteService<ContentBlock, Guid>
    {
        public ContentBlockService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<ContentBlock> validator, ILogger<ContentBlockService> logger) : base(dbContext, configuration, userManager, validator, logger)
        {
        }

        protected override async Task<IQueryable<ContentBlock>> ApplyIdFilter(IQueryable<ContentBlock> queryable, Guid id)
        {
            return queryable.Where(x => x.Id == id);
        }

        protected override List<string> WriteRoles => new List<string> { ApplicationRoleNames.SuperAdmin, ApplicationRoleNames.ContentManager };

        public async Task<ContentData> GetContentData(string slug, Dictionary<string, string> tokens, bool onlyPages = false)
        {
            ContentBlock contentBlock = await _dbContext.ContentBlocks
                .Where(x => x.Slug == slug)
                .SingleOrDefaultAsync();

            if (contentBlock == null)
                throw new Exception("Unable to find content block named: " + slug);

            if (onlyPages && !contentBlock.IsPage)
                throw new Exception("Unable to find content block page named: " + slug);

            string title = contentBlock.Title ?? "";
            foreach (string token in tokens.Keys)
                title = title.Replace("%" + token + "%", tokens[token]);

            string contentText = contentBlock.Value ?? "";
            foreach (string token in tokens.Keys)
                contentText = contentText.Replace("%" + token + "%", tokens[token]);

            Markdown markdown = new Markdown();
            string contentHtml = markdown.Transform(contentText);

            return new ContentData()
            {
                Title = title,
                ContentText = contentText,
                ContentHtml = contentHtml
            };
        }

    }

}
