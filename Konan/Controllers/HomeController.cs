using Konan.App_Start;
using Konan.Context;
using Konan.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Konan.Hubs;
using Konan.Classes;
using System.CodeDom.Compiler;

namespace Konan.Controllers
{
    [Authorize]
    [CustomAuthorizeAttribute]
    public class HomeController : Controller
    {
        KonanDBContext _dc = new KonanDBContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }

        public PartialViewResult Notifications()
        {
            return PartialView("_NotificationsPartial");
        }

        public PartialViewResult SearchResults(string q)
        {
            string sessionId = Session["Id"].ToString();
            return PartialView("SearchResults", _dc.Accounts.Where(m => m.FirstName.Contains(q) || m.LastName.Contains(q)).Where(m => m.Id != sessionId).Take(10).ToList());
        }  

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult FriendRequests(string id)
        {
            if(id != null)
            {
                var friendRequest = new FriendRequest();
                friendRequest.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                friendRequest.Id_A = Session["Id"].ToString();
                friendRequest.Id_F = id;

                _dc.FriendRequests.Add(friendRequest);
                _dc.SaveChanges();

                NotificationHub nH = new NotificationHub();
                nH.RegisterRequest(friendRequest.Id);

                return Json(true);
            }

            return Json(false);
        }

        [HttpPost]
        public JsonResult DeleteNotification(string id)
        {
            var like = _dc.Likes.Where(m => m.Id == id).FirstOrDefault();

            if(like != null)
            {
                like.Status = "saw";
                _dc.Entry(like).State = EntityState.Modified;
                _dc.SaveChanges();

                return Json(true);
            }
            else
            {
                var comment = _dc.Comments.Where(m => m.Id == id).FirstOrDefault();
                if (comment != null)
                {
                    comment.Status = "saw";
                    _dc.Entry(comment).State = EntityState.Modified;
                    _dc.SaveChanges();
                    return Json(true);
                }
                else
                {
                    var share = _dc.Posts.Where(p => p.Id == id).FirstOrDefault();
                    if(share != null)
                    {
                        share.Status = "saw";
                        _dc.Entry(share).State = EntityState.Modified;
                        _dc.SaveChanges();
                        return Json(true);
                    }
                }
            }          
          
            return Json(false);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dc.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}