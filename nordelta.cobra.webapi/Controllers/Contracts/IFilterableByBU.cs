using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.Contracts
{
    public interface IFilterableByBU
    {
        string GetBU();
    }
}
