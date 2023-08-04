using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class FilterReportByDatesViewModelRequest
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        
    }
}
