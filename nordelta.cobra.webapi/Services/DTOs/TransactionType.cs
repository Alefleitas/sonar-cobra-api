using System.Runtime.Serialization;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public enum TransactionType
    {
        [EnumMember(Value = "modifyCvuTransaction")]
        modifyCvuTransaction,
        [EnumMember(Value = "deleteCvuTransaction")]
        deleteCvuTransaction,
        [EnumMember(Value = "createCvuTransaction")]
        createCvuTransaction, 
        [EnumMember(Value = "operationTransaction")]
        operationTransaction
    }
}
