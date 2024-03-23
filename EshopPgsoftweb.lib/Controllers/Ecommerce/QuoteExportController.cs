using eshoppgsoftweb.lib.Tasks.Ecommerce;
using eshoppgsoftweb.lib.Util;
using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    [Authorize(Roles = "EcommerceAdmin")]
    public class QuoteExportController : _BaseController
    {
        public ActionResult GetQuoteMksoftXml(string id)
        {
            QuoteToXml export = new QuoteToXml(new Guid(id));

            ActionResult ret = DataDownloadResult.GetActionResult(XmlUtil.ToWin1250(export.Xml), string.Format("Objednavka{0}.xml", export.Quote.QuoteId));
            if (ret == null)
            {
                throw new HttpException(404, "Error generating XML");
            }

            return ret;
        }
    }
}
