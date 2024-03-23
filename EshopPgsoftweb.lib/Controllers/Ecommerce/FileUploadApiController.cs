using eshoppgsoftweb.lib.Repositories;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class FileUploadApiController : UmbracoApiController
    {
        [HttpPost]
        public object UploadFile()
        {
            FileUploadRepository fu = new FileUploadRepository();
            return fu.UploadFile();
        }

        public object ManageFiles(string id)
        {
            string[] items = id.Split('|');
            switch (items[0].ToLower())
            {
                case "delete":
                    {
                        FileUploadRepository fu = new FileUploadRepository();
                        return fu.DeleteFile(items[1]);
                    }
                case "description":
                    {
                        FileUploadRepository fu = new FileUploadRepository();
                        return fu.SetFileDescription(items[1], items[2], items[3]);
                    }
                default:
                    {
                        FileUploadRepository fu = new FileUploadRepository();
                        return fu.GetFiles(items[1]);
                    }
            }
        }
    }
}
