using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Utilities.Serialization
{
    public class IncludeInDTOAttribute : Attribute
    {
        public IncludeInDTOAttribute()
        {
        }

        public string Context { get; set; }

        public string[] Properties { get; set; }
    }
}
