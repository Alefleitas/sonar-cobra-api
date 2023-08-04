using nordelta.cobra.webapi.Utils;
using System;
using System.Xml;

namespace nordelta.cobra.webapi.Models;

public class ValidacionCliente
{
    public int Id { get; set; }
    public string PartyId { get; set; }
    public string RegistryId { get; set; }
    public string PartyCreationDate { get; set; }
    public string OrgOrigSystemReference { get; set; }
    public string RegistrationNumber { get; set; } // Cuit
    public string PartyName { get; set; }
    public string CustomerFlag { get; set; }
    public string SupplierFlag { get; set; }
    public string VendorNumber { get; set; }
    public string JgzzFiscalCode { get; set; }
    public string TaxRegimeCode { get; set; }
    public string RegistrationTypeCode { get; set; }
    public string DefaultRegistrationFlag { get; set; }
    public string AccountNumber { get; set; } // IdCliente
    public string AccOrigSystemReference { get; set; }
    public string SiteNumber { get; set; }
    public string CasOrigSystemReference { get; set; }
    public string PartySiteOsdr { get; set; }
    public string LocationOsdr { get; set; }
    public string LocAttribute1 { get; set; } // CodigoProducto
    public string SiteUseNumber { get; set; } // IdSiteCliente
    public string RefAcctBu { get; set; }
    public DateTime RegistryDate { get; set; } = LocalDateTime.GetDateTimeNow();
}
