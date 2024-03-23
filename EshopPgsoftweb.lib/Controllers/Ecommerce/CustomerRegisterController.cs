using dufeksoft.lib.Mail;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class CustomerRegisterController : _BaseController
    {
        public ActionResult Registration()
        {
            return View(GetRegisterModelForEdit());
        }

        [HttpPost]
        public ActionResult SubmitRegistration(CustomerRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsDeliveryAddress)
                {
                    if (string.IsNullOrEmpty(model.DeliveryName))
                    {
                        ModelState.AddModelError("DeliveryName", "Doručovacia firma (meno a priezvisko) musí byť zadané.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryCountryCollectionKey) || model.DeliveryCountryCollectionKey == Guid.Empty.ToString())
                    {
                        ModelState.AddModelError("DeliveryCountryCollectionKey", "Doručovacia krajina musí byť zadaná.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryStreet))
                    {
                        ModelState.AddModelError("DeliveryStreet", "Doručovacia ulica a číslo domu musí byť zadané.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryCity))
                    {
                        ModelState.AddModelError("DeliveryCity", "Doručovacia obec musí byť zadaná.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryZip))
                    {
                        ModelState.AddModelError("DeliveryZip", "Doručovacie PSČ musí byť zadané.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryPhone))
                    {
                        ModelState.AddModelError("DeliveryPhone", "Doručovací telefón musí byť zadaný.");
                    }
                }
                if (model.RegisterPassword != model.RepeatRegisterPassword)
                {
                    ModelState.AddModelError("RegisterPassword", "Heslo a Opakujte heslo musia byť rovnaké.");
                    ModelState.AddModelError("RepeatRegisterPassword", "Heslo a Opakujte heslo musia byť rovnaké.");
                }
                if (!model.AgreePersonalDataProfiling)
                {
                    ModelState.AddModelError("AgreePersonalDataProfiling", "Musíte označiť súhlas so spracovaním osobných údajov profilovaním.");
                }
                if (!new ApiKeyValidator().IsValid(model.Password, model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Musíte označiť, že nie ste robot.");
                }
            }
            if (ModelState.IsValid)
            {
                EshoppgsoftwebMember newMember = EshoppgsoftwebMemberModel.CreateCopyFrom(model);

                EshoppgsoftwebMemberRepository memberRep = new EshoppgsoftwebMemberRepository();
                EshoppgsoftwebMember member = null;
                MembershipCreateStatus status = memberRep.Save(this, newMember);
                if (status != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError("", string.Format("Nastala chyba pri zápise záznamu do systému. {0}. Opravte chyby a skúste akciu zopakovať. Ak sa chyba vyskytne znovu, kontaktujte nás prosím.", memberRep.GetErrorMessage(status)));
                }
                else
                {
                    member = memberRep.GetMemberByEmail(model.Email);
                }
                if (member != null)
                {
                    EshoppgsoftwebCustomerRepository customerRep = new EshoppgsoftwebCustomerRepository();
                    EshoppgsoftwebCustomer customer = null;
                    if (customerRep.Save(EshoppgsoftwebCustomer.CreateCopyFrom(model, member)))
                    {
                        //new EshoppgsoftwebNewsletterRepository().SetForEmail(model.Email, model.AgreePersonalDataNewsletter);
                        customer = customerRep.GetForOwner(member.MemberId);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nastala chyba pri zápise údajov. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                    }
                }
                if (!ModelState.IsValid)
                {
                    if (status == MembershipCreateStatus.Success && member != null)
                    {
                        // On error delete member
                        memberRep.Delete(member);
                    }
                    return CurrentUmbracoPage();
                }


                List<TextTemplateParam> paramList = new List<TextTemplateParam>();
                paramList.Add(new TextTemplateParam("LOGIN", newMember.Email));
                paramList.Add(new TextTemplateParam("LOGIN_URL", string.Format("{0}/moj-ucet/prihlasenie", new _BaseControllerUtil().SiteRootUrl)));

                // Odoslanie uzivatelovi
                EshoppgsoftwebMailer.SendMailTemplateWithoutBcc(
                    "Potvrdenie registrácie",
                    TextTemplate.GetTemplateText("NewRegistration_Sk", paramList),
                    newMember.Email);

                return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceRegistrationOkFormId);
            }

            return CurrentUmbracoPage();
        }

        CustomerRegisterModel GetRegisterModelForEdit()
        {
            CustomerRegisterModel model = new CustomerRegisterModel();
            model.DropDowns = new RegisterDropDowns();

            return model;
        }


        public ActionResult LostPassword()
        {
            return View(new LostPasswordModel());
        }

        [HttpPost]
        public ActionResult SubmitLostPassword(LostPasswordModel model)
        {
            TempData["success"] = null;

            if (ModelState.IsValid)
            {
                EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();
                EshoppgsoftwebMember member = repository.GetMemberByEmail(model.Email);
                if (member == null)
                {
                    ModelState.AddModelError("", "Užívateľ pre zádanú e-mailovú adresu neexistuje.");
                }
                else
                {
                    DateTime dt = DateTime.Now;
                    string pswd = dt.Ticks.ToString();

                    member.IsLockedOut = false;
                    member.IsApproved = true;
                    member.Password = pswd;
                    member.PasswordRepeat = pswd;
                    MembershipCreateStatus status = repository.Save(this, member, true);
                    if (status == MembershipCreateStatus.Success)
                    {
                        status = repository.SavePassword(member);
                    }
                    if (status != MembershipCreateStatus.Success)
                    {
                        ModelState.AddModelError("", string.Format("Nastala chyba pri zápise záznamu do systému. {0}. Opravte chyby a skúste akciu zopakovať. Ak sa chyba vyskytne znovu, kontaktujte nás prosím.", repository.GetErrorMessage(status)));
                    }
                    else
                    {
                        try
                        {
                            List<TextTemplateParam> paramList = new List<TextTemplateParam>();
                            paramList.Add(new TextTemplateParam("LOGIN", member.Email));
                            paramList.Add(new TextTemplateParam("PASSWORD", member.Password));

                            // Odoslanie uzivatelovi
                            EshoppgsoftwebMailer.SendMailTemplateWithoutBcc(
                                "Obnovenie prístupu na naplnspajzu.sk",
                                TextTemplate.GetTemplateText("LostPassword_Sk", paramList),
                                member.Email);

                            TempData["success"] = true;
                        }
                        catch (Exception exc)
                        {
                            ModelState.AddModelError("", exc.ToString());
                        }
                    }
                }
            }
            return CurrentUmbracoPage();
        }
    }
}
