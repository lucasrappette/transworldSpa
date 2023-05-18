using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Utilities.Serialization
{
    public class ExcludeFromDTOAttribute : Attribute
    {
        public ExcludeFromDTOAttribute()
        {
        }

        public string Context { get; set; }

        public string UnlessContext { get; set; }

        public string[] Properties { get; set; }

        public bool IsExcludedFromContext(string context)
        {
            if (!string.IsNullOrEmpty(UnlessContext))
                return context != UnlessContext;

            if (!string.IsNullOrEmpty(Context))
                return context == Context;

            return true;
        }
    }
}
