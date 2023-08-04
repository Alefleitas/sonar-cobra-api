
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class PublishDebinViewModel
    {
        public PublishDebinViewModel()
        {
            this.DebtAmounts = new List<DebtViewModel>();
        }
        //Get VendedorCBU with this cuit
        public string VendedorCuit { get; set; }

        public string Comprobante { get; set; }
        public double Importe { get; set; }
        public Currency Moneda { get; set; }

        //You can get the full BankAccount object, using this cbu and the userCuit from token
        public string CompradorCbu { get; set; }

        public List<DebtViewModel> DebtAmounts { get; set; } //DetalleDeudas ids
    }
    public class DebtViewModel
    {
        public int DebtId { get; set; }
        public double Amount { get; set; }
    }
}
