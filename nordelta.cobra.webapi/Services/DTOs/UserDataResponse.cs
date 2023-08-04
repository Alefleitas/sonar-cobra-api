using System;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class UserDataResponse
    {
        public UserDataResponse()
        {
            UserDataCuits = new List<string>();
            Roles = new List<string>();
            Empresas = new List<string>();
        }
        public string IdApplicationUser { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string TipoUsuario { get; set; }
        public List<string> UserDataCuits { get; set; }
        public string Email { get; set; }
        public string NroCuenta { get; set; }
        public bool EsExtranjero { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Empresas { get; set; }
        public string ReferenciaCliente { get; set; }
    }
}
