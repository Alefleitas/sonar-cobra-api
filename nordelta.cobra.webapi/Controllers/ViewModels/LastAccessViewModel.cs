using System;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class LastAccessViewModel
    {
        public Guid? UserId { get; set; }
        public string Email { get; set; }
        public string UsuarioNombre { get; set; }
        public Guid? SistemaId { get; set; }
        public string SistemaNombre { get; set; }
  
        
        public DateTime? LastLogin { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}