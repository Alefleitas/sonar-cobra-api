using System.Globalization;

namespace nordelta.cobra.webapi.Helpers
{
    public static class StringToDecimalExtensions
    {
        public static decimal GetDecimal(this string value)
        {
            var decimalValue = default(decimal);
            var englishCultureInfo = new CultureInfo("en-US");
            var spanishCultureInfo = new CultureInfo("es-AR");
            if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint, englishCultureInfo, out decimalValue))
            {
                return decimalValue;
            }
            if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint, spanishCultureInfo, out decimalValue))
            {
                return decimalValue;
            }
            if (decimal.TryParse(value, (NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands), englishCultureInfo, out decimalValue))
            {
                return decimalValue;
            }
            if (decimal.TryParse(value, (NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands), spanishCultureInfo, out decimalValue))
            {
                return decimalValue;
            }
            return decimalValue;
        }
        public static double GetDouble(this string value)
        {
            var doubleValue = default(double);
            var englishCultureInfo = new CultureInfo("en-US");
            var spanishCultureInfo = new CultureInfo("es-AR");
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, englishCultureInfo, out doubleValue))
            {
                return doubleValue;
            }
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, spanishCultureInfo, out doubleValue))
            {
                return doubleValue;
            }
            if (double.TryParse(value, (NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands), englishCultureInfo, out doubleValue))
            {
                return doubleValue;
            }
            if (double.TryParse(value, (NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands), spanishCultureInfo, out doubleValue))
            {
                return doubleValue;
            }
            return doubleValue;
        }
    }
}
