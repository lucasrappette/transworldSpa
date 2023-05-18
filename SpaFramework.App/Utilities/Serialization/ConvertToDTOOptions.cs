using SpaFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Utilities.Serialization
{
    public class ConvertToDTOOptions
    {
        public Casing Casing { get; set; } = Casing.Camel;

        public List<string> ExplicitIncludes { get; set; }
        public List<string> ExplicitExcludes { get; set; }

        public ConvertToDTOOptions() : this(null, null)
        {
        }

        public ConvertToDTOOptions(string explicitIncludes = null, string explicitExcludes = null)
        {
            ExplicitIncludes = (explicitIncludes ?? "").Split(",").ToList();
            ExplicitExcludes = (explicitExcludes ?? "").Split(",").ToList();
        }
    }
}
