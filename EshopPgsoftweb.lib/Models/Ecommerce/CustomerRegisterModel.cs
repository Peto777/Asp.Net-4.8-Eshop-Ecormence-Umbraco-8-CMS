using dufeksoft.lib.Model;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class CustomerRegisterModel : _BaseModel
    {
        [Required(ErrorMessage = "Firma (meno a priezvisko) musí byť zadané")]
        [Display(Name = "Firma (meno a priezvisko)")]
        public string Name { get; set; }
        [RequiredGuidDropDown(ErrorMessage = "Krajina musí byť zadaná")]
        [Display(Name = "Krajina")]
        public string CountryCollectionKey { get; set; }
        [Required(ErrorMessage = "Ulica a číslo domu musí byť zadané")]
        [Display(Name = "Ulica a číslo domu")]
        public string Street { get; set; }
        [Required(ErrorMessage = "Obec musí byť zadaná")]
        [Display(Name = "Obec")]
        public string City { get; set; }
        [Required(ErrorMessage = "PSČ musí byť zadané")]
        [Display(Name = "PSČ")]
        public string Zip { get; set; }
        [Required(ErrorMessage = "Telefón musí byť zadaný")]
        [Display(Name = "Telefón")]
        public string Phone { get; set; }

        [Email(ErrorMessage = "Neplatný tvar e-mailovej adresy")]
        [Required(ErrorMessage = "E-mail musí byť zadaný")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Ico(ErrorMessage = "Neplatné IČO")]
        [Display(Name = "IČO")]
        public string Ico { get; set; }
        [Dic(ErrorMessage = "Neplatné DIČ")]
        [Display(Name = "DIČ")]
        public string Dic { get; set; }
        [Icdph(ErrorMessage = "Neplatné IČ DPH")]
        [Display(Name = "IČ DPH")]
        public string Icdph { get; set; }
        [Display(Name = "Kontaktná osoba")]
        public string ContactName { get; set; }

        [Display(Name = "Adresa doručenia je iná ako fakturačná adresa")]
        public bool IsDeliveryAddress { get; set; }
        [Display(Name = "Doručovacia firma (meno a priezvisko)")]
        public string DeliveryName { get; set; }
        [Display(Name = "Doručovacia krajina")]
        public string DeliveryCountryCollectionKey { get; set; }
        [Display(Name = "Doručovacia ulica a číslo domu")]
        public string DeliveryStreet { get; set; }
        [Display(Name = "Doručovacia obec")]
        public string DeliveryCity { get; set; }
        [Display(Name = "Doručovacie PSČ")]
        public string DeliveryZip { get; set; }
        [Display(Name = "Doručovací telefón")]
        public string DeliveryPhone { get; set; }


        [Required(ErrorMessage = "Heslo musí byť zadané")]
        [Display(Name = "Heslo")]
        public string RegisterPassword { get; set; }
        [Required(ErrorMessage = "Opakujte heslo musí byť zadané")]
        [Display(Name = "Opakujte heslo")]
        public string RepeatRegisterPassword { get; set; }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public bool AgreePersonalDataProfiling { get; set; }
        public string IamNotRobotVirtualProperty { get; set; }


        public RegisterDropDowns DropDowns { get; set; }

        public CustomerRegisterModel()
        {
        }

        public CustomerRegisterModel(RegisterDropDowns dropDowns) : this()
        {
            this.DropDowns = dropDowns;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Name);
        }

        public string GetFullName()
        {
            return string.Format("{0}", this.Name);
        }
    }

    public class RegisterDropDowns
    {
        public CountryDropDown CountryCollection { get; set; }

        public RegisterDropDowns()
        {
            this.CountryCollection = CountryDropDown.CreateFromRepository(true);
        }
    }
}
