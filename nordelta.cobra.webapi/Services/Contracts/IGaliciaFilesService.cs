using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IGaliciaFilesService
    {
        Task CreateAndPublishDebtFilesToGaliciaAsync();
        Task GetAndProcessPaymentFilesOfGaliciaAsync();
    }
}
