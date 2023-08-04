using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IAutomaticDebinRepository
    {
        List<AutomaticPayment> All(User user);
        List<AutomaticPayment> All();
        AutomaticPayment Get(int Id);
        void Add(AutomaticPayment automaticPayment);
        void Delete(int Id);
    }
}
