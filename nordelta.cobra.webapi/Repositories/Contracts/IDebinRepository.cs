using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IDebinRepository
    {
        void Save(Debin debin);
        void UpdateAll(List<Debin> debinList);
        List<Debin> GetAllPending();
        List<Debin> GetAllPayed(string clientCuit);
        List<Debin> GetAllByIds(List<int> ids);
        List<Debin> GetAll();
    }
}
