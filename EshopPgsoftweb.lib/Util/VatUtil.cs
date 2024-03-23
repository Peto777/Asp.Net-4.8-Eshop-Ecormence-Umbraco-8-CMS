namespace eshoppgsoftweb.lib.Util
{
    public class VatUtil
    {
        public static decimal CalculatePriceWithoutVat(decimal priceWithVat, decimal vatPerc)
        {
            decimal tmp = 100M * (priceWithVat * 100M) / (100M + vatPerc);
            int tmpInt = (int)tmp;
            if (tmp - (decimal)tmpInt >= 0.5M)
            {
                tmpInt += 1;
            }

            return ((decimal)tmpInt) / 100M;
        }
    }
}
