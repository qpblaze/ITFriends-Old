using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Konan.Classes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                string id = HttpContext.Current.Session["Id"].ToString();
                string id2 = HttpContext.Current.Request.Cookies["AuthID"].Value;
            }
            catch(Exception)
            { 
                HandleUnauthorizedRequest(filterContext);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result =
           new RedirectToRouteResult(
               new RouteValueDictionary{{ "controller", "Account" },
                                          { "action", "SignOut" }

                                             });

        }
    }
}