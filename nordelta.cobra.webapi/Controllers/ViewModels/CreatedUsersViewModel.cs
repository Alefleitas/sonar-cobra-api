using System;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class CreatedUsersViewModel
    {
        public Guid Id { get; set; }
        public string Cuit { get; set; }
        public string Email { get; set; }
        public string RazonSocial { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
