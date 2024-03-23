using dufeksoft.lib.Mail;
using dufeksoft.lib.Model;
using eshoppgsoftweb.lib.Util;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models
{
    public class EshoppgsoftwebContactModel
    {
        [Required(ErrorMessage = "Priezvisko a meno musí byť zadané")]
        [Display(Name = "Priezvisko a meno")]
        public string Name { get; set; }
        [Email(ErrorMessage = ModelUtil.invalidEmailErrMessage_Sk)]
        [Required(ErrorMessage = "E-mail musí byť zadaný")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Text správy musí byť zadaný")]
        [Display(Name = "Text správy")]
        public string Text { get; set; }

        [Display(Name = "Heslo")]
        public string Password { get; set; }
        [Display(Name = "Potvrdenie hesla")]
        public string ConfirmPassword { get; set; }

        public bool SendContactRequest()
        {
            List<TextTemplateParam> paramList = new List<TextTemplateParam>();
            paramList.Add(new TextTemplateParam("NAME", this.Name));
            paramList.Add(new TextTemplateParam("EMAIL", this.Email));
            paramList.Add(new TextTemplateParam("TEXT", this.Text));

            // Odoslanie uzivatelovi
            EshoppgsoftwebMailer.SendMailTemplate(
                "Vaša správa",
                TextTemplate.GetTemplateText("ContactSendSuccess_Sk", paramList),
                this.Email, null);

            return true;
        }
    }
}
