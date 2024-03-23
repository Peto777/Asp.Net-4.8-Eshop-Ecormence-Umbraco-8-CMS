using eshoppgsoftweb.lib.Models.UmbracoCmsContent;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace eshoppgsoftweb.lib.Repositories.UmbracoCmsContent
{
    public class TextyRepository : _BaseRepository
    {
        public const int TextyId = 1060;

        public static Texty GetFromUmbraco(UmbracoHelper umbraco)
        {
            IPublishedContent content = umbraco.Content(TextyId);

            return content == null ? null : new Texty(content);
        }
    }
}
