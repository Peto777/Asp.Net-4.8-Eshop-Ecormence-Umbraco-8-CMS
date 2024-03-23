using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class MemberController : _BaseController
    {
        public ActionResult Login(string returnUrl)
        {
            //_UpdateRepository.CheckRepository();

            LoginModel model = new LoginModel();
            model.ReturnUrl = returnUrl;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitLogin(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Members.Login(model.Username, model.Password))
                {
                    // Update quote prices after login
                    //int memberId = Members.GetByUsername(model.Username).Id;
                    //QuoteModel.RecalcQuoteForSession(this.CurrentSessionId, memberId.ToString());
                    // redirect to desired page
                    if (string.IsNullOrEmpty(returnUrl) && !string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        returnUrl = model.ReturnUrl;
                    }
                    UrlHelper myHelper = new UrlHelper(HttpContext.Request.RequestContext);
                    if (myHelper.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceAfterLoginFormId);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Zadané užívateľské meno alebo heslo je nesprávne.");
                }
            }
            return CurrentUmbracoPage();
        }

        public ActionResult SubmitLogout()
        {
            // Logout
            TempData.Clear();
            Session.Clear();
            Members.Logout();

            // Update quote prices after logout
            //QuoteModel.RecalcQuoteForSession(this.CurrentSessionId, null);

            // Redirect to desired page
            return RedirectToCurrentUmbracoPage();
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "Name", string sortDir = "ASC")
        {
            EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();

            return View(EshoppgsoftwebMemberListModel.CreateCopyFrom(repository.GetAll(sort, sortDir)));
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View(new EshoppgsoftwebMemberModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            string isCustomerEdit = null;
            if (!string.IsNullOrEmpty(id))
            {
                string[] par = id.Split('|');
                id = par[0];
                if (par.Length > 1 && par[1] == ParamUtil.IsCustomerEdit)
                {
                    isCustomerEdit = ParamUtil.IsCustomerEdit;
                }
            }

            EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();
            EshoppgsoftwebMemberModel model = EshoppgsoftwebMemberModel.CreateCopyFrom(repository.Get(id));
            model.IsCustomerEdit = isCustomerEdit;

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(EshoppgsoftwebMemberModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (model.IsNew)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Heslo musí byť zadané.");
                }
                if (string.IsNullOrEmpty(model.PasswordRepeat))
                {
                    ModelState.AddModelError("PasswordRepeat", "Zopakované heslo musí byť zadané.");
                }
                if (model.Password != model.PasswordRepeat)
                {
                    ModelState.AddModelError("PasswordRepeat", "Heslo a zopakované heslo musia byť rovnaké.");
                }
            }

            if (ModelState.IsValid)
            {
                EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();
                MembershipCreateStatus status = repository.Save(this, EshoppgsoftwebMemberModel.CreateCopyFrom(model));
                if (status != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError("", string.Format("Nastala chyba pri zápise záznamu do systému. {0}. Opravte chyby a skúste akciu zopakovať. Ak sa chyba vyskytne znovu, kontaktujte nás prosím.", repository.GetErrorMessage(status)));
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return string.IsNullOrEmpty(model.IsCustomerEdit) ? this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceMembersFormId) : this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceCustomersFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();
            EshoppgsoftwebMemberModel model = EshoppgsoftwebMemberModel.CreateCopyFrom(repository.Get(id));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(EshoppgsoftwebMemberModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();
            MembershipCreateStatus status = repository.Delete(EshoppgsoftwebMemberModel.CreateCopyFrom(model));
            if (status != MembershipCreateStatus.Success)
            {
                ModelState.AddModelError("", string.Format("Nastala chyba pri zápise záznamu do systému. {0}. Opravte chyby a skúste akciu zopakovať. Ak sa chyba vyskytne znovu, kontaktujte nás prosím.", repository.GetErrorMessage(status)));
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceMembersFormId);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditPassword(string id)
        {
            string isCustomerEdit = null;
            if (!string.IsNullOrEmpty(id))
            {
                string[] par = id.Split('|');
                id = par[0];
                if (par.Length > 1 && par[1] == ParamUtil.IsCustomerEdit)
                {
                    isCustomerEdit = ParamUtil.IsCustomerEdit;
                }
            }

            EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();
            EshoppgsoftwebMemberModel model = EshoppgsoftwebMemberModel.CreateCopyFrom(repository.Get(id));
            model.IsCustomerEdit = isCustomerEdit;

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SavePassword(EshoppgsoftwebMemberModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Heslo musí byť zadané.");
            }
            if (string.IsNullOrEmpty(model.PasswordRepeat))
            {
                ModelState.AddModelError("PasswordRepeat", "Zopakované heslo musí byť zadané.");
            }
            if (model.Password != model.PasswordRepeat)
            {
                ModelState.AddModelError("", "Heslo a zopakované heslo musia byť rovnaké.");
            }

            if (ModelState.IsValid)
            {
                EshoppgsoftwebMemberRepository repository = new EshoppgsoftwebMemberRepository();
                MembershipCreateStatus status = repository.SavePassword(EshoppgsoftwebMemberModel.CreateCopyFrom(model));
                if (status != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError("", string.Format("Nastala chyba pri zápise záznamu do systému. {0}. Opravte chyby a skúste akciu zopakovať. Ak sa chyba vyskytne znovu, kontaktujte nás prosím.", repository.GetErrorMessage(status)));
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return string.IsNullOrEmpty(model.IsCustomerEdit) ? this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceMembersFormId) : this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceCustomersFormId);
        }
    }
}
