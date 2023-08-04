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
    public class AccountBalance : IFilterableByDepartment, IFilterableByBU
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ClientId { get; set; }
        [ForeignKey("ClientId")]
        public User Client { get; set; }
        public List<Communication> Communications { get; set; }
        [Required]
        public string Product { get; set; }
        [Required]
        public string ClientCuit { get; set; }
        [Required]
        public double TotalDebtAmount { get; set; }
        [Required]
        public int FuturePaymentsCount { get; set; }
        [Required]
        public double FuturePaymentsAmountUSD { get; set; }
        public string OverduePaymentDate { get; set; }
        [Required]
        public int OverduePaymentsCount { get; set; }
        [Required]
        public double OverduePaymentsAmountUSD { get; set; }
        [Required]
        public int PaidPaymentsCount { get; set; }
        [Required]
        public double SalesInvoiceAmountUSD { get; set; }
        [Required]
        public double PaidPaymentsAmountUSD { get; set; }
        [Required]
        public EDepartment Department { get; set; }
        [Required]
        public EBalance Balance { get; set; }
        public EDelayStatus? DelayStatus { get; set; }
        [Required]
        public EContactStatus ContactStatus { get; set; }
        public string BusinessUnit { get; set; }
        public string PublishDebt { get; set; }
        public string WorkStarted { get; set; }
        public string RazonSocial { get; set; }
        public string ClientReference { get; set; }

        public virtual List<CvuEntity> CvuEntities { get; set; }

        public string GetBU()
        {
            return BusinessUnit;
        }

        public EDepartment GetDepartment()
        {
            return Department;
        }

        public string GetPublishDebt()
        {
            return PublishDebt;
        }

        public enum EContactStatus
        {
            NoContactado = 0,
            Contactado = 1
        }

        public enum EDepartment
        {
            CuentasACobrar = 0,
            Legales = 1,
            Externo = 2
        }

        public enum EBalance
        {
            AlDia = 0,
            Mora = 1
        }

        public enum EDelayStatus
        {
            Negociacion = 0,
            CartaDocumento = 1,
            Juicio = 2,
            Incobrable = 3
        }
    }

    public class AcountBalanceEntity
    {
        public const string
            Department = "Department",
            DelayStatus = "DelayStatus",
            WorkStarted = "WorkStarted",
            PublishDebt = "PublishDebt";
    }
}
