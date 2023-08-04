using System.Runtime.Serialization;

namespace nordelta.cobra.webapi.Models
{
    public enum Currency
    {
        [EnumMember(Value = "ARS")]
        ARS = 0,
        [EnumMember(Value = "USD")]
        USD = 2,
        [EnumMember(Value = "UVA")]
        UVA = 4,
        [EnumMember(Value = "EUR")]
        EUR = 6
    }
}
