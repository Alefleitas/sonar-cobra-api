using nordelta.cobra.webapi.Repositories.Contexts;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    [Auditable]
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public EPermission Code { get; set; }
    }

    public enum EPermission
    {
        Access_Payments = 1,
        Access_MyAccountBalance = 2,
        Access_Contact = 3,
        Access_EverybodysPayments = 4,
        Access_Configuration = 5,
        Access_Extern_Payments = 6,
        Access_CRM = 7,
        Access_Support = 8,
        Access_Quotations = 9,
        Access_Rates = 10,
        Access_Templates = 11,
        Access_EverybodysPaymentsCriba = 12,
        Access_FAQ = 13,
        Create_Quotation = 14,
        Access_Debt_Post = 15,
        Access_Automatic_Debt = 16,
        Generate_Payment = 17,
        Access_Reports = 18,
        Access_AdvancePayments = 19,
        Lock_AdvancePayments = 20,
        Inform_Manually = 21,
        Access_Admin_AdvancePayments = 22,
        Access_Admin_Payments = 23,
        Access_Debt_Free = 24
    }
}
