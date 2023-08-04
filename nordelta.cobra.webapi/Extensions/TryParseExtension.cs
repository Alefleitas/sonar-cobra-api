using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Extensions
{
    public static class TryParseExtension
    {
        public static int? TryParse(this string Source)
        {
            int result;
            if (int.TryParse(Source, out result))
                return result;
            else

                return null;
        }
    }
}
