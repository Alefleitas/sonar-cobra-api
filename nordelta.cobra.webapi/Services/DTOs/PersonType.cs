using System.Runtime.Serialization;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public enum PersonType
    {
        [EnumMember(Value = "Fisica")]
        Fisica,
        [EnumMember(Value = "Juridica")]
        Juridica
    }
}
