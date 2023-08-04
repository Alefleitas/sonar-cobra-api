
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class ExternDebinViewModel
    {
        public string VendedorCuit { get; set; }
        //Get VendedorCBU with this cuit
        public string Vendedor { get; set; } //Razón social (ej. "Nordelta SA")
        public string CodigoProducto { get; set; } //descripcion de lo que se compra
        public double Importe { get; set; }
        public Currency Moneda { get; set; }
        //You can get the full BankAccount object, using this cbu and the userCuit from token
        public string CBU { get; set; }
        public string CompradorCuit { get; set; }
        public string CompradorNombre { get; set; }
        public string Sistema { get; set; }
    }
}
