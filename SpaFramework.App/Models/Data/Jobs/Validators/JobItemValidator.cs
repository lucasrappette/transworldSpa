using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Jobs.Validators
{
    public class JobItemValidator : AbstractValidator<JobItem>
    {
        public JobItemValidator()
        {
        }
    }
}
