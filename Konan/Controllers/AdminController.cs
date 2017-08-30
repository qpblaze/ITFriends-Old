using Konan.Classes;
using Konan.Context;
using Konan.Models;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Konan.Controllers
{
    [Authorize]
    [CustomAuthorizeAttribute]
    public class AdminController : Controller
    {
        KonanDBContext _dc = new KonanDBContext();

        public ActionResult Index()
        {
            if (Session["Email"].ToString() == "ursaciuc.adrian27@gmail.com" && Session["Password"].ToString() == Account.PasswordHash("parola123-"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        public PartialViewResult Accounts()
        {
            return PartialView("_AccountsPartial", _dc.Accounts.OrderBy(a => a.FirstName).ToList());
        }

        public PartialViewResult Others()
        {
            return PartialView("_OthersPartial");
        }


        public PartialViewResult Posts()
        {
            return PartialView("_PostsPartial", _dc.Posts.OrderBy(a => a.Date).ToList());
        }

        public JsonResult DeleteAccount(string id)
        {
            Account account = _dc.Accounts.Where(a => a.Id == id).FirstOrDefault();
            
            if(account != null)
            {
                List<Like> likes = _dc.Likes.Where(l => l.Id_A == account.Id).ToList();
                foreach (var l in likes)
                {
                    _dc.Likes.Remove(l);
                    _dc.SaveChanges();
                }

                List<Comment> comments = _dc.Comments.Where(c => c.Id_A == account.Id).ToList();
                foreach (var c in comments)
                {
                    _dc.Comments.Remove(c);
                    _dc.SaveChanges();
                }

                List<Post> shares = _dc.Posts.Where(s => s.Id_S == account.Id || s.Id_A == account.Id).ToList();
                foreach (var s in shares)
                {
                    _dc.Posts.Remove(s);
                    _dc.SaveChanges();
                }

                List<Friend> friends = _dc.Friends.Where(f => f.Id_A == account.Id || f.Id_F == account.Id).ToList();
                foreach (var f in friends)
                {
                    _dc.Friends.Remove(f);
                    _dc.SaveChanges();
                }

                List<FriendRequest> friendRequests = _dc.FriendRequests.Where(f => f.Id_A == account.Id || f.Id_F == account.Id).ToList();
                foreach (var f in friendRequests)
                {
                    _dc.FriendRequests.Remove(f);
                    _dc.SaveChanges();
                }

                List<Chat> messages = _dc.Messages.Where(m => m.FromId == account.Id || m.ToId == account.Id).ToList();
                foreach (var m in messages)
                {
                    _dc.Messages.Remove(m);
                    _dc.SaveChanges();
                }

                _dc.Accounts.Remove(account);
                _dc.SaveChanges();

                return Json(true);
            }

            return Json(false);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit()
        {

            return View();
        }

        public ActionResult DeleteMessages()
        {
            _dc.Database.ExecuteSqlCommand("TRUNCATE TABLE Chat");

            return RedirectToAction("Index", "Admin");
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