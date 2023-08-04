using System;
using nordelta.cobra.webapi.Repositories.Contexts;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.Contracts;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class UserChangesLog 
    {
        [Key]
        public int Id { get; set; }
        public int EntityId { get; set; }
        public string ModifiedEntity { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ModifiedField { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }

    public class CobraEntity
    {
        public const string
            AccountBalance = "AccountBalance";
        public const string
            ContactDetail = "ContactDetail";
        public const string
            BankAccount = "BankAccount";
        public const string
            Quotations = "Quotations";
        public const string
            AdvanceFee = "AdvanceFee";
        public const string
            LockAdvancePayments = "LockAdvancePayments";
        public const string
            Communication = "Communication";
    }
}
