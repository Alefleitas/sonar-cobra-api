using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IItauArchivosService
    {
        Task GetAndProcessPaymentFilesOfItau();
        void GetAndProcessPaymentFilesCVUOfItauLocal();
        void GetAndProcessPaymentFilesECHEQOfItauLocal();
    }
}
