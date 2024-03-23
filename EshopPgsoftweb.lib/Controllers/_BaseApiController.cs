using System.Web;
using Umbraco.Web.WebApi;

namespace eshoppgsoftweb.lib.Controllers
{
    public class _BaseApiController : UmbracoApiController
    {
        public string CurrentSessionId
        {
            get
            {
                return HttpContext.Current.Session.SessionID;
            }
        }

        public string GetRequestParam(string paramName)
        {
            return HttpContext.Current.Request.Params.Get(paramName);
        }
    }
}
