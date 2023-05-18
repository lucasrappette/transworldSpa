using FluentValidation;
using SpaFramework.App.Models.Data.Dealers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Dealers.Validators
{
    public class DealerValidator : AbstractValidator<Dealer>
    {
        public DealerValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50);
        }
    }
}
