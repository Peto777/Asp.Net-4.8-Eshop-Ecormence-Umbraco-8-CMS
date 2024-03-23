using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Util;
using System.Web;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers
{
    public class _BaseController : SurfaceController
    {
        public string DefaultImgPath
        {
            get
            {
                return this.HttpContext.Server.MapPath("~/Styles/Images");
            }
        }
        public string CurrentSessionId
        {
            get
            {
                return HttpContext.Session.SessionID;
            }
        }
        public HttpRequest CurrentRequest
        {
            get
            {
                return new _BaseControllerUtil().CurrentRequest;
            }
        }
        protected RedirectToUmbracoPageResult RedirectToEshoppgsoftwebUmbracoPage(string pageKey)
        {
            return this.RedirectToUmbracoPage(GetPageId(pageKey));
        }
        protected RedirectToUmbracoPageResult RedirectToEshoppgsoftwebUmbracoPage(string pageKey, string queryString)
        {
            return this.RedirectToUmbracoPage(GetPageId(pageKey), queryString);
        }

        int GetPageId(string pageKey)
        {
            return ConfigurationUtil.GetPageId(pageKey);
        }

        public void SetSeoModel(_SeoModel seo)
        {
            this.TempData[_SeoModel.TemDataKey] = seo;
        }

        public void SetCurrentProductCategoryModel(CategoryPublicModel currentProductCategory)
        {
            GetCurrentEshopModel().CurrentProductCategory = currentProductCategory;
        }
        public _EshopModel GetCurrentEshopModel()
        {
            if (!this.TempData.ContainsKey(_EshopModel.TemDataKey))
            {
                this.TempData[_EshopModel.TemDataKey] = new _EshopModel() { CurrentProductCategory = null };
            }

            return (_EshopModel)this.TempData[_EshopModel.TemDataKey];
        }
    }

    public class _BaseControllerUtil
    {
        public string CurrentSessionId
        {
            get
            {
                return HttpContext.Current.Session.SessionID;
            }
        }
        public HttpRequest CurrentRequest
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }

        public string SiteRootUrl
        {
            get
            {
                System.Uri uri = this.CurrentRequest.Url;

                return string.Format("{0}://{1}{2}",
                    uri.Scheme,
                    uri.Host,
                    uri.IsDefaultPort ? "" : string.Format(":{0}", uri.Port));
            }
        }

        public string GetAbsoluteUrl(string relativeUrl)
        {
            return string.Format("{0}{1}", this.SiteRootUrl, relativeUrl);
        }
    }
}
