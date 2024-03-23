using eshoppgsoftweb.lib.Models;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers
{
    [PluginController("EshopPgsoftweb")]
    public class ContactController : _BaseController
    {
        public ActionResult Index()
        {
            return View("ContactForm", new EshoppgsoftwebContactModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleFormSubmit(EshoppgsoftwebContactModel model)
        {
            if (ModelState.IsValid)
            {
                if (!new ApiKeyValidator().IsValid(model.Password, model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Musíte označiť, že nie ste robot.");
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            TempData["success"] = model.SendContactRequest();

            return RedirectToCurrentUmbracoPage();
        }
    }
}
