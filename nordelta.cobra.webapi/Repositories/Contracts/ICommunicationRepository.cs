using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface ICommunicationRepository
    {
        IUnitOfWork UnitOfWork { get; }
        Communication InsertOrUpdate(Communication comm);
        List<Communication> GetAllForAccountBalance(int accountBalanceId);
        bool Delete(int id, User user);
        Communication GetById(int id);
        List<Communication> GetAll();
        bool Insert(IEnumerable<Communication> communications);
        bool ToggleTemplate(int id);
    }
}
