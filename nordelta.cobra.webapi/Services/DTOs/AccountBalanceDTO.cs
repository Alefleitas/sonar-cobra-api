using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.Contracts;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class AccountBalanceDTO : IFilterableByDepartment
    {
        public int Id { get; set; }
        public AccountBalance.EDepartment Department { get; set; }
        public AccountBalance.EDelayStatus? DelayStatus { get; set; }
        public AccountBalance.EContactStatus ContactStatus { get; set; }
        public string PublishDebt { get; set; }
        public string WorkStarted { get; set; }
        public string Product { get; set; }
        public string ClientCuit { get; set; }

        public AccountBalance.EDepartment GetDepartment()
        {
            return Department;
        }
        public string GetPublishDebt()
        {
            return PublishDebt;
        }
    }
}
