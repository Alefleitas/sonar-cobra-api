using SQLite;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace nordelta.cobra.webapi.Models
{
    [Table("HolidayDay")]
    public class HolidayDay 
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Dia { get; set; }
        public int Mes { get; set; }
        public string Motivo { get; set; }
        public int Anio { get; set; }
    }
}
