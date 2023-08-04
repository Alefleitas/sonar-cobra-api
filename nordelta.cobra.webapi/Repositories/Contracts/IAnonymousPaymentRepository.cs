using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IAnonymousPaymentRepository
    {
        List<AnonymousPayment> GetPendingForMigration();
        List<AnonymousPayment> GetAllPending();
        void UpdateAll(List<AnonymousPayment> externDebinList);
        void Save(AnonymousPayment anonymousPayment);
        void CheckMigrated(int Id);
        Task<AnonymousPayment> GetByDebinCode(string debinCode);
    }
}
