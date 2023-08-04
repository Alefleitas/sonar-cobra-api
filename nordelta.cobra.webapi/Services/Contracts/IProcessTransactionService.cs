using nordelta.cobra.webapi.Models.ValueObject.BankFiles;
using nordelta.cobra.webapi.Services.DTOs;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IProcessTransactionService
    {
        bool ProcessTransactionResult(TransactionResultDto transactionResult, string cuitPsp);
        void ProcessRegistroFiles(IEnumerable<FileRegistro> fileRegistro);


    }
}
