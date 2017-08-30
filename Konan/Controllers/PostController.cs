using Konan.Classes;
using Konan.Context;
using Konan.Hubs;
using Konan.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using CloudinaryDotNet.Actions;

namespace Konan.Controllers
{
    [Authorize]
    [CustomAuthorizeAttribute]
    public class PostController : Controller
    {
        KonanDBContext _dc = new KonanDBContext();

        public ActionResult CreatePost()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreatePost(Post model)
        {
            if (ModelState.IsValid)
            {
                using (KonanDBContext _dc = new KonanDBContext())
                {
                    model.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                    model.Id_A = Session["Id"].ToString();
                    model.Date = DateTime.Now;

                    var acc = _dc.Accounts.Where(a => a.Id == model.Id_A).FirstOrDefault();
                    acc.Points += 5;

                    _dc.Entry(acc).State = EntityState.Modified;
                    _dc.SaveChanges();

                    if (model.Description != null)
                    {
                        Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);

                        MatchCollection mactches = regx.Matches(model.Description);

                        foreach (Match match in mactches)
                        {
                            model.Description = model.Description.Replace(match.Value, "<a href='" + match.Value + "'>" + match.Value + "</a>");
                        } 
                    }
                    

                    if (model.File != null)
                    {
                        if (model.File.ContentLength > 0)
                        {
                            CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(
                              "ursaciuc-adrian",
                              "473916817971436",
                              "S_S0CSKbWFMVh9-Se8ZXKxWLQLg");

                            CloudinaryDotNet.Cloudinary cloudinary = new CloudinaryDotNet.Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(model.File.FileName, model.File.InputStream)
                            };
                            var uploadResult = cloudinary.Upload(uploadParams);
                            
                            model.ImageUrl = uploadResult.Uri.OriginalString;
                        }
                    }
                    _dc.Posts.Add(model);
                    _dc.SaveChanges();
                }
            }
            return Json("success");
        }
        
        public JsonResult Edit(string id, string title, string description)
        {
            Post post = _dc.Posts.Where(p => p.Id == id).FirstOrDefault();
            if(post != null)
            {
                post.Title = title;
                post.Description = description;

                _dc.Entry(post).State = EntityState.Modified;
                _dc.SaveChanges();

                return Json(true);
            }
            return Json(false);

            
        }

        public ActionResult ViewPost(string id)
        {
            if(id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.IdPost = id;
            return View();
        }

        public PartialViewResult Posts(string id, string idPost)
        {
            
            if(idPost == null)
            {
                if (id == null)
                {
                    string sessionId = Session["Id"].ToString();
                    var friends = _dc.Friends.Where(m => m.Id_A == sessionId || m.Id_F == sessionId).ToList();
                    List<Post> posts = new List<Post>();
                    foreach (var f in friends)
                    {
                        if (f.Id_A == Session["Id"].ToString())
                        {
                            posts.AddRange(_dc.Posts.Where(m => m.Id_A == f.Id_F && m.Visibility != "private"));
                        }
                        else
                        {
                            posts.AddRange(_dc.Posts.Where(m => m.Id_A == f.Id_A && m.Visibility != "private"));
                        }
                    }
                    posts.AddRange(_dc.Posts.Where(m => m.Id_A == sessionId));

                    return PartialView("Posts", posts.OrderByDescending(m => m.Date));
                }
                else
                {
                    if(id == Session["Id"].ToString())
                        return PartialView("Posts", _dc.Posts.Where(m => m.Id_A == id).OrderByDescending(m => m.Date).ToList());
                    else
                    {
                        string sessionId = Session["Id"].ToString();
                        var friends = _dc.Friends.Where(m => (m.Id_A == sessionId && m.Id_F == id) || (m.Id_F == sessionId && m.Id_A == id)).FirstOrDefault();
                        if (friends != null)
                        {
                            return PartialView("Posts", _dc.Posts.Where(m => m.Id_A == id && m.Visibility != "private").OrderByDescending(m => m.Date).ToList());
                        }
                        else
                        {
                            return PartialView("Posts", _dc.Posts.Where(m => m.Id_A == id && m.Visibility != "private" && m.Visibility != "friends").OrderByDescending(m => m.Date).ToList());
                        }
                    }
                        
                }
            }
            else
            {
                return PartialView("Posts", _dc.Posts.Where(m => m.Id == idPost).OrderByDescending(m => m.Date).ToList());
            }
            
        }

        public ActionResult DeleteComment(string id)
        {
            if(id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var comment = _dc.Comments.Find(id);
            if(comment != null)
            {
                if (comment.Id_A == Session["Id"].ToString())
                {
                    _dc.Comments.Remove(comment);
                    _dc.SaveChanges();
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var post = _dc.Posts.Find(id);

            if (post != null)
            {
                string idS = Session["Id"].ToString();
                Account admin = _dc.Accounts.Where(a => a.Id == idS).FirstOrDefault();
                if (post.Id_A == Session["Id"].ToString() || Session["Id"].ToString() == admin.Id)
                {
                    var dLikes = _dc.Likes.Where(m => m.Id_P == id).ToList();
                    foreach (var i in dLikes)
                    {
                        _dc.Likes.Remove(i);
                    }

                    var dComments = _dc.Comments.Where(m => m.Id_P == id).ToList();
                    foreach (var i in dComments)
                    {
                        _dc.Comments.Remove(i);
                    }

                    var dShares = _dc.Posts.Where(m => m.Id_S == id).ToList();
                    foreach (var i in dShares)
                    {
                        _dc.Posts.Remove(i);
                    }

                    var dSolutions = _dc.Solutions.Where(s => s.Id_P == id).ToList();
                    _dc.Solutions.RemoveRange(dSolutions);

        
                    
                    _dc.Posts.Remove(post);
                    _dc.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult Like(string id)
        {
            if (id == null)
            {
                return Json(false);
            }

            string sessionId = Session["Id"].ToString();
            var like = _dc.Likes.Where(m => m.Id_P == id && m.Id_A == sessionId).FirstOrDefault();

            if (like == null)
            {
                like = new Like();
                like.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");

                like.Id_A = Session["Id"].ToString();
                like.Id_P = id;

            

                _dc.Likes.Add(like);
                _dc.SaveChanges();

                var post = _dc.Posts.Where(p => p.Id == id).FirstOrDefault();
                var acc = _dc.Accounts.Where(a => a.Id == post.Id_A).FirstOrDefault();
                acc.Points += 1;

                _dc.Entry(acc).State = EntityState.Modified;
                _dc.SaveChanges();
                if (post.Id_A != like.Id_A)
                {
                    NotificationHub nH = new NotificationHub();
                    nH.RegisterLike(like.Id);
                }
            }
            else
            {
                _dc.Likes.Remove(like);
                _dc.SaveChanges();
            }
            return Json(true);
        }

        public JsonResult Share(string id)
        {
            if(id != null)
            {
                var oldPost = _dc.Posts.Where(m => m.Id == id).FirstOrDefault();

                if (oldPost.Account.Id == Session["Id"].ToString())
                {
                    return Json(false);
                }

                var newPost = new Post();

                newPost.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                newPost.Id_A = Session["Id"].ToString();
                newPost.Title = oldPost.Title;
                newPost.Description = oldPost.Description;
                newPost.ImageUrl = oldPost.ImageUrl;
                newPost.Date = DateTime.Now;
                newPost.Id_S = id;

                var acc = _dc.Accounts.Where(a => a.Id == oldPost.Id_A).FirstOrDefault();
                acc.Points += 5;

                _dc.Entry(acc).State = EntityState.Modified;
                _dc.SaveChanges();

                NotificationHub nH = new NotificationHub();
                nH.RegisterShare(id);
           

                _dc.Posts.Add(newPost);
                _dc.SaveChanges();
            }

            return Json(false);
        }

        public ActionResult Comments(string id)
        {
            return PartialView("Comments", _dc.Comments.Where(m => m.Id_P == id).OrderBy(m => m.Date).ToList());
        }

        public JsonResult AddComment(string id, string comment)
        {
            if(!string.IsNullOrEmpty(comment))
            {
                Comment com = new Comment();
                com.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_") .Replace("+", "-");
                com.Id_A = Session["Id"].ToString();
                com.CommentT = comment;
                com.Id_P = id;
                com.Date = DateTime.Now.ToString();

                _dc.Comments.Add(com);
                _dc.SaveChanges();

                var post = _dc.Posts.Where(p => p.Id == id).FirstOrDefault();
                var acc = _dc.Accounts.Where(a => a.Id == post.Id_A).FirstOrDefault();
                acc.Points += 3;

                _dc.Entry(acc).State = EntityState.Modified;
                _dc.SaveChanges();
                if (post.Id_A != com.Id_A)
                {
                    NotificationHub nH = new NotificationHub();
                    nH.RegisterComment(com.Id);
                }
            }
            

            return Json("success");
        }

        public JsonResult ChangeVisibility(string id, string vis)
        {
            if (id != null && vis != null)
            {
                var post = _dc.Posts.Where(p => p.Id == id).FirstOrDefault();
                if(post != null)
                {
                    post.Visibility = vis;
                    _dc.Entry(post).State = EntityState.Modified;
                    _dc.SaveChanges();
                }
               return Json(true);
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