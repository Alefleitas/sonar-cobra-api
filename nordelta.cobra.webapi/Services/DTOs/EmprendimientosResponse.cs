using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class EmprendimientosResponse
    {
        public string codigo { get; set; }
        public string emprendimiento { get; set; }
        public decimal precioPactadoValor { get; set; }
    }
}
