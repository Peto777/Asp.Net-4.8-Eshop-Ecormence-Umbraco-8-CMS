using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class SysConstModel : _BaseModel
    {
        [Required(ErrorMessage = "Spoločnosť musí byť zadaná")]
        [Display(Name = "Spoločnosť")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "IČO musí byť zadané")]
        [Display(Name = "IČO")]
        public string CompanyIco { get; set; }
        [Required(ErrorMessage = "DIČ musí byť zadané")]
        [Display(Name = "DIČ")]
        public string CompanyDic { get; set; }
        [Required(ErrorMessage = "IČ DPH musí byť zadané")]
        [Display(Name = "IČ DPH")]
        public string CompanyIcdph { get; set; }

        [Required(ErrorMessage = "Ulica a číslo domu musí byť zadané")]
        [Display(Name = "Ulica a číslo domu")]
        public string AddressStreet { get; set; }
        [Required(ErrorMessage = "Obec musí byť zadaná")]
        [Display(Name = "Obec")]
        public string AddressCity { get; set; }
        [Required(ErrorMessage = "PSČ musí byť zadané")]
        [Display(Name = "PSČ")]
        public string AddressZip { get; set; }
        [Required(ErrorMessage = "Telefón musí byť zadaný")]
        [Display(Name = "Telefón")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "E-mail musí byť zadaný")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Banka musí byť zadaná")]
        [Display(Name = "Banka")]
        public string Bank { get; set; }
        [Required(ErrorMessage = "IBAN musí byť zadaný")]
        [Display(Name = "IBAN")]
        public string Iban { get; set; }
        [Required(ErrorMessage = "Mena musí byť zadaná")]
        [Display(Name = "Mena")]
        public string Currency { get; set; }
        [Required(ErrorMessage = "Suma objednávky pre bezplatnú dopravu musí byť zadaná")]
        [Display(Name = "Suma objednávky pre bezplatnú dopravu")]
        public string FreeTransportPrice { get; set; }

        public SysConstModel()
        {
        }

        public void CopyDataFrom(SysConst src)
        {
            this.pk = src.pk;

            this.CompanyName = src.CompanyName;
            this.CompanyIco = src.CompanyIco;
            this.CompanyDic = src.CompanyDic;
            this.CompanyIcdph = src.CompanyIcdph;

            this.AddressStreet = src.AddressStreet;
            this.AddressCity = src.AddressCity;
            this.AddressZip = src.AddressZip;
            this.Email = src.Email;
            this.Phone = src.Phone;

            this.Bank = src.Bank;
            this.Iban = src.Iban;
            this.Currency = src.Currency;
            this.FreeTransportPrice = PriceUtil.NumberToEditorString(src.FreeTransportPrice);
        }

        public void CopyDataTo(SysConst trg)
        {
            trg.pk = this.pk;

            trg.CompanyName = this.CompanyName;
            trg.CompanyIco = this.CompanyIco;
            trg.CompanyDic = this.CompanyDic;
            trg.CompanyIcdph = this.CompanyIcdph;

            trg.AddressStreet = this.AddressStreet;
            trg.AddressCity = this.AddressCity;
            trg.AddressZip = this.AddressZip;
            trg.Email = this.Email;
            trg.Phone = this.Phone;

            trg.Bank = this.Bank;
            trg.Iban = this.Iban;
            trg.Currency = this.Currency;
            trg.FreeTransportPrice = PriceUtil.NumberFromEditorString(this.FreeTransportPrice);
        }

        public static SysConstModel CreateCopyFrom(SysConst src)
        {
            SysConstModel trg = new SysConstModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static SysConst CreateCopyFrom(SysConstModel src)
        {
            SysConst trg = new SysConst();
            src.CopyDataTo(trg);

            return trg;
        }
    }
}
