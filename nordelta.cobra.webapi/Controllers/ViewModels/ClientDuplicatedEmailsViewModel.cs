using System;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class ClientDuplicatedEmailsViewModel
    {
        public Guid Id { get; set; }
        public string Cuit { get; set; }
        public string Email { get; set; }
        public string RazonSocial { get; set; }
    }
}