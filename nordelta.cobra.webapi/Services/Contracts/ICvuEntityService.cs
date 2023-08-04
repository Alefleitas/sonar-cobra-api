using System.Collections.Generic;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface ICvuEntityService
    {
        bool CompleteCvuCreationForTransactionId(string transactionId, TransactionResultDto transactionResult);
        bool CreateCvu(CvuEntityDto newCvu);
        bool BeginCvuTransactionCreation(int accountBalanceId, string productCode, string cuitBu, string clientCuit);
        void CvuMassCreationProcess();
        string GetCuitBuFromTransactionId(string transactionId);
        IEnumerable<CvuEntityDto> GetCvuEntitiesByIdAccounBalance(int IdAccountBalance);
        CvuEntity GetById(int cvuEntityId);
    }
}
