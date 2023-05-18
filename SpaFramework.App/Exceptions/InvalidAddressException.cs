using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Exceptions
{
    public class InvalidAddressException : BadRequestException
    {
        public string ShortDescription { get; set; }

        public InvalidAddressException() : base()
        {
        }

        public InvalidAddressException(string shortDescription, string correctiveAction) : base(shortDescription + " " + correctiveAction)
        {
            ShortDescription = shortDescription;
        }
    }
}
