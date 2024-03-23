using eshoppgsoftweb.lib.Repositories;
using System;
using System.IO;
using System.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class EcommerceApiController : UmbracoApiController
    {
        // ~/Umbraco/EcommerceApi/ClearSessionData
        public string ClearSessionData()
        {
            try
            {
                EshoppgsoftwebUserPropRepository rep = new EshoppgsoftwebUserPropRepository();
                rep.DeleteOldSessionData(DateTime.Now.AddDays(-1));

                QuoteRepository quoteRep = new QuoteRepository();
                quoteRep.DeleteOldSessionData(DateTime.Now.AddDays(-7));

                DeleteOldLogFiles(DateTime.Now.AddDays(-7));
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(EcommerceApiController), "ClearSessionData error", exc);
                return "ERR";
            }

            return "OK";
        }

        private void DeleteOldLogFiles(DateTime dt)
        {
            string logPath = string.Format("{0}\\App_Data\\Logs",
                HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath));

            foreach (FileInfo fi in new DirectoryInfo(logPath).GetFiles())
            {
                if (fi.LastWriteTime < dt)
                {
                    fi.Delete();
                }
            }
        }
    }
}
