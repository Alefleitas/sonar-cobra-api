using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ChangePasswordResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
