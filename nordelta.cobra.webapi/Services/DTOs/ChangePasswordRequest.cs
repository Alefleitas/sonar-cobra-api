using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string SsoToken { get; set; }
    }
}
