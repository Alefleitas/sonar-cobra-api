using nordelta.cobra.webapi.Models;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class NotificationTypeViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public Template Template { get; set; }
    }
}