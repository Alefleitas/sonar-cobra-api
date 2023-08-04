using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace nordelta.cobra.webapi.Models
{
    public class Template
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public Boolean Disabled { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string HtmlBody { get; set; }
    }
}
