using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace nordelta.cobra.webapi.Models
{
    [Table("SsoEmpresa")]
    public class SsoEmpresa
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string SsoId { get; set; }
        public string Nombre { get; set; }
        public string IdBusinessUnit { get; set; }
        public string Firma { get; set; }
        public string Correo { get; set; }
    }
}
