using System;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Utils
{
    public class PersonTypeCalc
    {
        public static PersonType GetPersonTypeFromCuit(string cuit)
        {
            if (cuit.Length <= 2) throw new Exception("Cuit tamaño incorrecto!");

            var firstNumber = cuit.Substring(0, 2);
            return firstNumber switch
            {
                "30" => PersonType.Juridica,
                "33" => PersonType.Juridica,
                _ => PersonType.Fisica
            };
        }

    }
}
