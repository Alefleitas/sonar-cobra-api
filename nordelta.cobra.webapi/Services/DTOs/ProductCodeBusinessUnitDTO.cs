using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.Contracts;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ProductCodeBusinessUnitDTO
    {
        public string Codigo { get; set; }
        public string BusinessUnit { get; set; }
        public string BusinessUnitCuit { get; set; }
    }
}
