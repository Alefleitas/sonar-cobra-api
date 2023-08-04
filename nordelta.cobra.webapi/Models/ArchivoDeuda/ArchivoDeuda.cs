using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models.ArchivoDeuda
{
    public class ArchivoDeuda
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TimeStamp { get; set; }

        public string FormatedFileName { get; set; }
        public string FileName { get; set; }
        public HeaderDeuda Header { get; set; }
        public TrailerDeuda Trailer { get; set; }
    }
}
