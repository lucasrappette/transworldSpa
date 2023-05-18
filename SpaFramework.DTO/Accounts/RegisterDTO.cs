using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Accounts
{
    public class RegisterDTO
    {
        /// <summary>
        /// Username for the registering user
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Email address for the registering user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password for the registering user
        /// </summary>
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
