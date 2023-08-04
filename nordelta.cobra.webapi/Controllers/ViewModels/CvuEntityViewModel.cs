using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class CvuEntityViewModel
    {
        public int Id { get; set; }
        public string ItauCreationTransactionId { get; set; }
        public string CvuValue { get; set; }
        public string Alias { get; set; }
        public int AccountBalanceId { get; set; }
        public Currency Currency { get; set; }

    }
}
