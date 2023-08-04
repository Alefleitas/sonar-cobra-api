using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IAutomaticPaymentsService
    {
        void ExecutePaymentsFor(DateTime date);
    }
}
