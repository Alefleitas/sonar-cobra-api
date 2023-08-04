using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    public class DepartmentChangeNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CodigoProducto { get; set; }
        [Required]
        public string RazonSocial { get; set; }
        [Required]
        public string NumeroCuitCliente { get; set; }
    }
}
