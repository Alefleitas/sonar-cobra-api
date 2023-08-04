using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ContactDetailDto
    {
		public string DocumentNumber { get; set; }
		public string PartyName { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string Cp { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string EmailAddress { get; set; }
		public string Producto { get; set; }
	}
}
