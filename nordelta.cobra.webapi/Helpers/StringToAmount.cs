namespace nordelta.cobra.webapi.Helpers
{
    public static class StringToAmount
    {
        public static double GetAmount(this string value)
        {
            var doubleValue = default(double);

            if (value.Length > 2)
            {
                int intPartEndIndex = value.Length - 2;
                string strAmount = value[..intPartEndIndex] + "." + value.Substring(intPartEndIndex, 2);
                doubleValue = strAmount.GetDouble();
            }

            return doubleValue;
        }
    }
}
