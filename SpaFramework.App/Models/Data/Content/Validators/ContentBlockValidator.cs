using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Content.Validators
{
    public class ContentBlockValidator : AbstractValidator<ContentBlock>
    {
        public ContentBlockValidator()
        {
            RuleFor(x => x.Slug).NotNull().NotEmpty();
        }
    }
}
