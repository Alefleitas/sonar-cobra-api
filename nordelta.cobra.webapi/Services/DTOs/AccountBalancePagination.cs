using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class AccountBalancePagination
    {
        public AccountBalancePagination()
        {
            AccountBalances = new List<AccountBalance>();
        }
        public List<AccountBalance> AccountBalances { get; set; }
        public int TotalCount { get; set; }
    }
}
