using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface ISantanderFilesService
    {
        void GetAndProcessPaymentFilesOfSantander();
        Task CreateAndPublishDebtFilesToSantanderAsync();
    }
}
