using FluentValidation;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Donors.Validators
{
    public class ApplicationRoleValidator : AbstractValidator<ApplicationRole>
    {
        public ApplicationRoleValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }
}
