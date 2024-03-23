using System.Web.Security;

namespace eshoppgsoftweb.lib.Models
{
    public class MainNavigationModel
    {
        public MembershipUser User { get; set; }
        public _EshopModel Eshop { get; set; }
    }
}
