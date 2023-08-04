using nordelta.cobra.webapi.Services;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    public abstract class DeliveryType
    {
        [Key]
        public int Id { get; set; }
        public string Configuration { get; set; }
        [NotMapped]
        public dynamic Service { get; set; }
        public abstract void Send(SsoUser recipient, string subject, string message);
    }
}
