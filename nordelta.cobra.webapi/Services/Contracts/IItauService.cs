using DebitoInmediatoServiceItau;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IItauService
    {
        Task<RegistrarPublicacionResponse> RegisterPublicationAsync(RegisterPublicationDTO debinData, bool isAnonymousPayment = false);
        Task<PaymentStatus> GetDebinState(GetDebinStateRequest debinStateRequest);
        void CheckCertificateExpirationDate();
        OperationInformationResultDto CallItauGetOperationInformation(string getUri, string cuitPsp);
        TransactionResultDto CallItauApiGetCvuInformation(string getUri, string cuitPsp);
        TransactionResultDto CallItauApiCreateTransaction(RegisterCvuDto registerCvuDto, string cuitPsp);

    }
}
