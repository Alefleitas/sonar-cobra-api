using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Utils
{
    public static class CbuValidator
    {
        public static bool IsValid(string cbu) {
            try
            {
                if (string.IsNullOrEmpty(cbu) || string.IsNullOrWhiteSpace(cbu))
                    return false;

                if (cbu.ElementAt(7).ToString() != GetCheckSumCbu(cbu.Substring(0, 7)))
                {
                    return false;
                }

                if (cbu.ElementAt(21).ToString() != GetCheckSumCbu(cbu.Substring(8, 13)))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetCheckSumCbu(string value)
        {
            var ponderador = new int[] { 3, 1, 7, 9 };
            var sum = 0;
            var j = 0;

            for (int i = value.Length - 1; i >= 0; --i)
            {
                var a = int.Parse(value.ElementAt(i).ToString());
                sum += (a * ponderador[j % 4]);
                ++j;
            }

            return ((10 - sum % 10) % 10).ToString();
        }
    }
}
