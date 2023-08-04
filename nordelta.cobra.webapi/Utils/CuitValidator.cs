using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Utils
{
    public static class CuitValidator
    {
        public static bool IsValid(string cuit) {
            try
            {
                if (string.IsNullOrEmpty(cuit) || string.IsNullOrWhiteSpace(cuit))
                    return false;

                var result = 0;
                for (int i = 0; i <= 9; i++)
                {
                    var a = int.Parse(cuit.ElementAt(9 - i).ToString());
                    result += a * (2 + (i % 6));
                }
                var checksum = 11 - (result % 11);
                checksum = checksum == 11 ? 0 : checksum;
                return checksum.ToString() == cuit.Last().ToString();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
