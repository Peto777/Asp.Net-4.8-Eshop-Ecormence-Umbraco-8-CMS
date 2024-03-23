using System.Globalization;

namespace eshoppgsoftweb.lib.Util
{
    public class PriceUtil
    {
        public static string GetPriceString(decimal price, bool trimDecZeros = false)
        {
            return GetPriceStringWithCurrency(trimDecZeros ? NumberToEditorString(price) : NumberToTwoDecString(price));
        }
        public static string GetUserFriendlyPriceString(decimal price)
        {
            string ret = NumberToEditorString(price);
            if (ret.IndexOf(',') > 0)
            {
                string[] items = ret.Split(',');
                string beforeComma = items[0];
                string afterComma = items[1];
                if (afterComma.Length > 2)
                {
                    int lastNonZeroIdx = afterComma.Length - 1;
                    for (int i = afterComma.Length - 1; i > 1; i--)
                    {
                        if (afterComma[i] != '0')
                        {
                            lastNonZeroIdx = i;
                            break;
                        }
                    }
                    afterComma = afterComma.Substring(0, lastNonZeroIdx + 1);
                }

                ret = string.Format("{0},{1}", beforeComma, afterComma);
            }

            return ret;
        }
        public static string GetPriceStringWithCurrency(string priceString)
        {
            return string.Format("{0} {1}", priceString, SysConstUtil.AppConst.Currency);
        }

        public static string GetPercString(decimal perc)
        {
            return string.Format("{0} %", NumberToEditorString(perc));
        }

        public static string NumberToTwoDecString(decimal price)
        {
            string ret = string.Format("{0:0.00}", price).Replace(".", ",");

            return ret;
        }

        public static string NumberToEditorString(decimal price)
        {
            string ret = price.ToString(NumberFormatInfo.InvariantInfo).Replace(".", ",");
            if (ret.EndsWith(",00"))
            {
                ret = ret.Substring(0, ret.LastIndexOf(",00"));
            }

            return ret;
        }

        public static decimal NumberFromEditorString(string str)
        {
            decimal price = 0M;

            if (!decimal.TryParse(str.Replace(",", "."), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out price))
            {
                price = 0M;
            }

            return price;
        }
    }
}
