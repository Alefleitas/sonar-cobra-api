using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class RegisterPublicationDTO
    {
        public Company Vendedor { get; set; }
        public DateTime Now { get; set; }
        public DateTime DueDate { get; set; }
        public Currency Moneda { get; set; }
        public string CompradorCuit { get; set; }
        public string CompradorCbu { get; set; }
        public string CompradorNombre { get; set; }
        public string Producto { get; set; }
        public double Importe { get; set; }
        public Guid ExternalCode { get; set; }
        public string ExternalCodeString => RemoveSpecialCharacters(Convert.ToBase64String(ExternalCode.ToByteArray())[..22]);

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled).PadLeft(22, '0');
        }
    }
}
