using eshoppgsoftweb.lib.Repositories;

namespace eshoppgsoftweb.lib.Util
{
    public class SysConstUtil
    {
        private static object _lock = new object();
        private static SysConst sysConst = null;

        public static SysConst AppConst
        {
            get
            {
                return GetSysConst();
            }
        }

        public static void Clear()
        {
            lock (SysConstUtil._lock)
            {
                SysConstUtil.sysConst = null;
            }
        }

        public static SysConst GetSysConst()
        {
            lock (SysConstUtil._lock)
            {
                if (SysConstUtil.sysConst == null)
                {
                    SysConstRepository rep = new SysConstRepository();
                    SysConstUtil.sysConst = rep.Get();
                }
            }

            return SysConstUtil.sysConst;
        }

        public static bool IsFreeTransportPriceAvailable
        {
            get
            {
                return SysConstUtil.AppConst.FreeTransportPrice > 0;
            }
        }

        public static bool IsVatPaier
        {
            get
            {
                return !string.IsNullOrEmpty(SysConstUtil.AppConst.CompanyIcdph);
            }
        }

        /// <summary>
        /// Vypocita cenu, ktora chyba pre dosiahnutie dopravy zdarma
        /// </summary>
        /// <param name="price">Aktualna cena</param>
        /// <returns></returns>
        public static decimal GetFreeTransportPrice(decimal price)
        {
            decimal priceTwoDecPlaces = PriceUtil.NumberFromEditorString(PriceUtil.NumberToTwoDecString(price));
            decimal freeTransportPrice = SysConstUtil.AppConst.FreeTransportPrice;
            if (priceTwoDecPlaces < freeTransportPrice)
            {
                // Vypocitaj rozdiel pre dopravu zdarma
                return freeTransportPrice - priceTwoDecPlaces;
            }

            // Cena uz je dostatocna, je narok na dopravu zdarma
            return 0M;
        }
    }
}
