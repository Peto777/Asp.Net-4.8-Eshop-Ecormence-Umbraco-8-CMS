using eshoppgsoftweb.lib.Util;
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace eshoppgsoftweb.lib
{
    public class EshoppgsoftwebApp : UmbracoApplication
    {
        // Init. Set up handlers here.
        public override void Init()
        {
            HttpApplication objApplication = this as HttpApplication;

            objApplication.PreRequestHandlerExecute += PreRequestHandlerExecute;

            base.Init();
        }

        // Called when a session starts.
        private new void PreRequestHandlerExecute(object sender, EventArgs e)
        {
            // Get current session.
            HttpSessionState objSession = ((UmbracoApplication)sender).Context.Session;

            // Make sure that there is an active session.
            if (objSession != null)
            {
                // Work with the session here.
                objSession["eshoppgsoftwebInit"] = 1;
            }
        }
    }

    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class UpdateContentFindersComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            ////add our custom MyContentFinder just before the core ContentFinderByUrl...
            //composition.ContentFinders().InsertBefore<ContentFinderByUrl, MyContentFinder>();
            ////remove the core ContentFinderByUrl finder:
            //composition.ContentFinders().Remove<ContentFinderByUrl>();
            ////you can use Append to add to the end of the collection
            //composition.ContentFinders().Append<AnotherContentFinderExample>();
            ////or Insert for a specific position in the collection
            //composition.ContentFinders().Insert<AndAnotherContentFinder>(3);
            composition.ContentFinders().Append<CategoryContentFinder>();
            composition.ContentFinders().Append<ProductContentFinder>();
        }
    }

    public class CategoryContentFinder : IContentFinder
    {
        public const string CategoryPath = "/eshop/kategoria/";
        public const string CategoryUrl_All = "-vsetko-";

        public bool TryFindContent(PublishedRequest contentRequest)
        {
            string path = contentRequest.Uri.AbsolutePath.ToLower();
            if (!IsCategoryPath(path))
                return false; // not found

            var contentCache = contentRequest.UmbracoContext.Content;
            var content = contentCache.GetById(ConfigurationUtil.GetPageId(ConfigurationUtil.Ecommerce_ProductPublic_CategoryPageId));
            if (content == null) return false; // not found

            // render that node
            contentRequest.PublishedContent = content;

            return true;
        }

        public static bool IsCategoryPath(string path)
        {
            return path.StartsWith(CategoryContentFinder.CategoryPath);
        }

        public static string GetCategoryUrl(Uri uri)
        {
            int segmentsCnt = uri.Segments.Count();
            if (segmentsCnt < 4)
            {
                return null;
            }

            StringBuilder str = new StringBuilder();
            for (int i = 3; i < segmentsCnt; i++)
            {
                str.Append(string.Format("{0}/", uri.Segments[i].TrimEnd('/')));
            }
            return str.ToString().TrimEnd('/');
        }
    }

    public class ProductContentFinder : IContentFinder
    {
        public const string ProductPath = "/eshop/produkt/";

        public bool TryFindContent(PublishedRequest contentRequest)
        {
            string path = contentRequest.Uri.AbsolutePath.ToLower();
            if (!path.StartsWith(ProductContentFinder.ProductPath))
                return false; // not found

            var contentCache = contentRequest.UmbracoContext.Content;
            var content = contentCache.GetById(ConfigurationUtil.GetPageId(ConfigurationUtil.Ecommerce_ProductPublic_DetailPageId));
            if (content == null) return false; // not found

            // render that node
            contentRequest.PublishedContent = content;

            return true;
        }

        public static string GetProductUrl(Uri uri)
        {
            if (uri.Segments.Count() < 4)
            {
                return null;
            }

            return uri.Segments[3].TrimEnd('/');
        }
    }
}
