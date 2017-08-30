using Konan.Hubs;
using Konan.Models;
using Konan.Context;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;
using Konan.Classes;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp;
using RestSharp.Authenticators;
using HttpCookie = System.Web.HttpCookie;
using CloudinaryDotNet.Actions;

namespace Konan.Controllers
{
    public class AccountController : Controller
    {
        KonanDBContext _dc = new KonanDBContext();
        public PartialViewResult Messages(string id)
        {
            string sessionId = Session["Id"].ToString();
            return PartialView("_MessagesPartial", _dc.Messages.Where(m => (m.FromId == sessionId && m.ToId == id) || (m.ToId == sessionId && m.FromId == id)).OrderBy(m => m.Date).ToList());

        }

        public JsonResult SendMessage(string idUser, string message)
        {
            if (idUser != null)
            {
                if(!string.IsNullOrEmpty(message) && !string.IsNullOrWhiteSpace(message))
                {
                    Chat msg = new Chat();
                    msg.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                    msg.ToId = idUser;
                    msg.FromId = Session["Id"].ToString();
                    msg.Message = message;
                    msg.Date = DateTime.Now;

                    _dc.Messages.Add(msg);
                    _dc.SaveChanges();

                    NotificationHub nH = new NotificationHub();
                    nH.RegisterMessage(msg.Id);
                }

                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        [AllowAnonymous]
        public ActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            
            Account account = _dc.Accounts.FirstOrDefault(a => a.ResetPassword == id);
            if (account == null)
            {
                return RedirectToAction("Index", "Home"); 
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            string id = Url.RequestContext.RouteData.Values["id"].ToString();

            Account account = _dc.Accounts.FirstOrDefault(a => a.ResetPassword == id);
            if(account != null)
            {
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if(string.Equals(model.NewPassword, model.ConfirmNewPassword))
                    {
                        account.Password = Account.PasswordHash(model.NewPassword);
                        account.ResetPassword = null;
                        _dc.Entry(account).State = EntityState.Modified;
                        _dc.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Password confirmation must match!");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Password can't be null.");
                    return View(model);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home"); 
            }
        }

        [AllowAnonymous]
        public ActionResult RecoverPassword()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        public string Shorten(string url)
        {
            string key = "AIzaSyDk6cOB5Jot47cu6BKH5T8Q1rVn-XKR0W0";

            string post = "{\"longUrl\": \"" + url + "\"}";
            string shortUrl = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=" + key);

            try
            {
                request.ServicePoint.Expect100Continue = false;
                request.Method = "POST";
                request.ContentLength = post.Length;
                request.ContentType = "application/json";
                request.Headers.Add("Cache-Control", "no-cache");

                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] postBuffer = Encoding.ASCII.GetBytes(post);
                    requestStream.Write(postBuffer, 0, postBuffer.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(responseStream))
                        {
                            string json = responseReader.ReadToEnd();
                            shortUrl = Regex.Match(json, @"""id"": ?""(?<id>.+)""").Groups["id"].Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return shortUrl;
        }

        public static RestResponse SendSimpleMessage(string email, string body)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator = new HttpBasicAuthenticator(
                "api", "key-81ca8cd4d5b72de5161a94bd750213b6");
            RestRequest request = new RestRequest();
            request.AddParameter("domain",
                "samples.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "sandbox592ca72a7ef04939889016836744cf32.mailgun.org/messages";
            request.AddParameter("from", "IT Friends <itfriends@ursaciuc-adrian.tech>");
            request.AddParameter("to", email);
            request.AddParameter("subject", "IT Friends - Reset Password");
            request.AddParameter("html", body);
            request.Method = Method.POST;
            return (RestResponse)client.Execute(request);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecoverPassword(RecoverPasswordModel model)
        {
            var url = "";
            try
            {
                var email = new EmailAddressAttribute();
                if (!email.IsValid(model.Email))
                {
                    ModelState.AddModelError("", "Please enter a valid email!");
                    return View(model);
                }

                Account account = _dc.Accounts.FirstOrDefault(a => a.Email == model.Email);

                if (account != null)
                {
                    account.ResetPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                    _dc.Entry(account).State = EntityState.Modified;
                    _dc.SaveChanges();
                    var currentURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host;
                    url += currentURL + "/Account/ResetPassword/" + account.ResetPassword;
                    string body = "<!DOCTYPE html> <html> <head> <title>Reset Password</title> <style> body{ font-family: 'Source Sans Pro', sans-serif; color: #7f8186; } .btn{ display: inline-block; padding: 6px 12px; margin-bottom: 0; font-size: 14px; font-weight: normal; line-height: 1.42857143; text-align: center; white-space: nowrap; vertical-align: middle; -ms-touch-action: manipulation; touch-action: manipulation; cursor: pointer; -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; user-select: none; background-image: none; border: 1px solid transparent; border-radius: 50px; background: #D54E40;\r\n    background: -moz-linear-gradient(left,  #D54E40 0%, #C41F4F 100%);\r\n    background: -webkit-linear-gradient(left,  #D54E40 0%,#C41F4F 100%); background: linear-gradient(to right,  #D54E40 0%,#C41F4F 100%);filter: progid:DXImageTransform.Microsoft.gradient( startColorstr=\'#D54E40\', endColorstr=\'#C41F4F\',GradientType=1 );} .text-center{ text-align: center; } .img-responsive { margin: 0 auto; } .btn{ width: 100%; font-weight: bold; font-size: 20px; height: 50px; background-color: #D54D3F; border-radius: 0; box-shadow: none; color: #fff; } .btn:hover, .btn:focus{ background-color: #D54D3F; color: #fff; } a{ color: #fff; } a:hover, a:focus{ text-decoration: none; color: #fff; } </style> </head> <body> <div class='container'> <div class='text-center'> </br> </br> <img src='http://i.imgur.com/t7DZoQs.png' class='img-responsive' width='200' height='200'><br> <h2>Hi " + account.FirstName + " " + account.LastName + "!</h2><br> <h4>We've recived a request to reset your password. If you didn't make the request,<br> just ignore this email. Otherwise, you can reset you password using this link:</h4><br> <a href=" + url + "><button class='btn btn-default'>Reset password</button></a> <h4>Thanks,</h4> <h4>IT Friends Team</h4> </div> </div> </body> </html>";
                    SendSimpleMessage(account.Email, body);
                    

                    //MailMessage mail = new MailMessage();
                    //mail.To.Add(model.Email);
                    //mail.From = new MailAddress("staff.konan@gmail.com", "IT Friends", System.Text.Encoding.UTF8);
                    //mail.Subject = "IT Friends - Reset Password";
                    //mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    //mail.Body = "<!DOCTYPE html> <html> <head> <title>Reset Password</title> <style> body{ font-family: 'Source Sans Pro', sans-serif; color: #7f8186; } .btn{ display: inline-block; padding: 6px 12px; margin-bottom: 0; font-size: 14px; font-weight: normal; line-height: 1.42857143; text-align: center; white-space: nowrap; vertical-align: middle; -ms-touch-action: manipulation; touch-action: manipulation; cursor: pointer; -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; user-select: none; background-image: none; border: 1px solid transparent; border-radius: 50px; background: #D54E40;\r\n    background: -moz-linear-gradient(left,  #D54E40 0%, #C41F4F 100%);\r\n    background: -webkit-linear-gradient(left,  #D54E40 0%,#C41F4F 100%); background: linear-gradient(to right,  #D54E40 0%,#C41F4F 100%);filter: progid:DXImageTransform.Microsoft.gradient( startColorstr=\'#D54E40\', endColorstr=\'#C41F4F\',GradientType=1 );} .text-center{ text-align: center; } .img-responsive { margin: 0 auto; } .btn{ width: 100%; font-weight: bold; font-size: 20px; height: 50px; background-color: #D54D3F; border-radius: 0; box-shadow: none; color: #fff; } .btn:hover, .btn:focus{ background-color: #D54D3F; color: #fff; } a{ color: #fff; } a:hover, a:focus{ text-decoration: none; color: #fff; } </style> </head> <body> <div class='container'> <div class='text-center'> </br> </br> <img src='http://i.imgur.com/t7DZoQs.png' class='img-responsive' width='200' height='200'><br> <h2>Hi " + account.FirstName + " " + account.LastName + "!</h2><br> <h4>We've recived a request to reset your password. If you didn't make the request,<br> just ignore this email. Otherwise, you can reset you password using this link:</h4><br> <a href=" + url + "><button class='btn btn-default'>Reset password</button></a> <h4>Thanks,</h4> <h4>IT Friends Team</h4> </div> </div> </body> </html>";
                    //mail.BodyEncoding = System.Text.Encoding.UTF8;
                    //mail.IsBodyHtml = true;
                    //mail.Priority = MailPriority.High;
                    //SmtpClient client = new SmtpClient();
                    //client.Credentials = new System.Net.NetworkCredential("staff.konan@gmail.com", "konanpass123-");
                    //client.Port = 587;
                    //client.Host = "smtp.gmail.com";
                    //client.EnableSsl = true;
                    //client.Send(mail);



                    
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "This email is not associated with a user.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "This service is blocked by the host.\nGo to: " + url);
                return View(model);
            }
            
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(SignInViewModel model)
        {
            if(ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("", "Please enter your email!");
                    return View(model);
                }
                else
                {
                    var email = new EmailAddressAttribute();
                    if(!email.IsValid(model.Email))
                    {
                        ModelState.AddModelError("", "Please enter a valid email!");
                        return View(model);
                    }
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("", "Please enter your password!");
                    return View(model);
                }
                if(Account.ValidAccount(model))
                {
                    Session["Email"] = model.Email;
                    Account a = _dc.Accounts.Where(e => e.Email == model.Email).FirstOrDefault();
                    if(a != null)
                    {
                        Session["Id"] = a.Id;
                        Session["Password"] = a.Password;
                        Session["FullName"] = a.FirstName + " " + a.LastName;
                        Session["FirstName"] = a.FirstName;
                        Session["LastName"] = a.LastName;

                        var cookie = new HttpCookie("AuthID");
                        cookie.Value = a.Id;
                        Response.Cookies.Add(cookie);

                        FormsAuthentication.SetAuthCookie(model.Email, false);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Session error.");
                    }
                    
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect email and/or password.");
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("", "Please enter your email!");
                    return View(model);
                }
                else
                {
                    var email = new EmailAddressAttribute();
                    if (!email.IsValid(model.Email))
                    {
                        ModelState.AddModelError("", "Please enter a valid email!");
                        return View(model);
                    }
   
                    if(!Account.ValidEmail(model.Email))
                    {
                        ModelState.AddModelError("", "This email is already in use!");
                        return View(model);
                    }
                    
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("", "Please enter your password!");
                    return View(model);
                }
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Password confirmation must match!");
                    return View(model);
                }
                if (string.IsNullOrEmpty(model.FirstName))
                {
                    ModelState.AddModelError("", "Please enter your first name!");
                    return View(model);
                }
                if (string.IsNullOrEmpty(model.LastName))
                {
                    ModelState.AddModelError("", "Please enter your last name!");
                    return View(model);
                }
                Account a = new Account();
                a.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                a.Email = model.Email;

                a.Password = Account.PasswordHash(model.Password);
                a.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.FirstName);
                a.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.LastName);
                a.ImageUrl = "/Img/no_avatar.png";

                _dc.Accounts.Add(a);
                _dc.SaveChanges();

                return RedirectToAction("SignIn", "Account");
                
            }
            return View(model);
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        [Authorize]
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("SignIn", "Account");
        }

        [Authorize]
        [CustomAuthorizeAttribute]
        public ActionResult ViewProfile(string id)
        {
            if(id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Id = id;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorizeAttribute]
        public ActionResult ViewProfile(EditProfileViewModel model)
        {
            ViewBag.Id = Session["Id"].ToString();
            string id = Session["Id"].ToString();
            if (string.IsNullOrEmpty(model.OldPassword) && string.IsNullOrEmpty(model.NewPassword) 
                && string.IsNullOrEmpty(model.ConfirmNewPassword) && model.File == null)
            {
                ModelState.AddModelError("", "No changes to save!");
                return View(model);
            }
            if(!string.IsNullOrEmpty(model.NewPassword) && string.IsNullOrEmpty(model.OldPassword))
            {
                ModelState.AddModelError("", "Please enter the old password!");
                return View(model);
            }

            Account account = _dc.Accounts.FirstOrDefault(a => a.Id == id);

            if (!string.IsNullOrEmpty(model.OldPassword) && !string.Equals(Account.PasswordHash(model.OldPassword), account.Password))
            {
                ModelState.AddModelError("", "The old password is incorrect!");
                return View(model);
            }
            else
            {

                if (!string.IsNullOrEmpty(model.OldPassword) && string.IsNullOrEmpty(model.NewPassword))
                {
                    ModelState.AddModelError("", "Please enter the new password!");
                    return View(model);
                }
                else
                {
                    if (!string.Equals(model.NewPassword, model.ConfirmNewPassword))
                    {
                        ModelState.AddModelError("", "Password confirmation must match!");
                        return View(model);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(model.OldPassword) && string.Equals(model.NewPassword, model.OldPassword))
                        {
                            ModelState.AddModelError("", "Your new password can't be same as old password!");
                            return View(model);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(model.OldPassword))
                            {
                                account.Password = Account.PasswordHash(model.NewPassword);

                                _dc.Entry(account).State = EntityState.Modified;
                                _dc.SaveChanges();
                            }              
                        }
                    }
                }
            }


            if (model.File != null)
            {
                if (model.File.ContentLength > 0)
                {
                    CloudinaryDotNet.Account cloud = new CloudinaryDotNet.Account(
                              "ursaciuc-adrian",
                              "473916817971436",
                              "S_S0CSKbWFMVh9-Se8ZXKxWLQLg");

                    CloudinaryDotNet.Cloudinary cloudinary = new CloudinaryDotNet.Cloudinary(cloud);

                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(model.File.FileName, model.File.InputStream)
                    };
                    var uploadResult = cloudinary.Upload(uploadParams);

                    model.ImageUrl = uploadResult.Uri.ToString();

                    string sessionId = Session["Id"].ToString();
                    var acc = _dc.Accounts.Find(sessionId);

                    acc.ImageUrl = model.ImageUrl;
                    _dc.Entry(acc).State = EntityState.Modified;
                    _dc.SaveChanges();
                }
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FriendRequest(string id, string submit)
        {

            var request = _dc.FriendRequests.Where(m => m.Id == id).FirstOrDefault();
            if (submit == "Accept")
            {
                var friend = new Friend();
                friend.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                friend.Id_A = Session["Id"].ToString();
                friend.Id_F = request.Id_A;

                _dc.Friends.Add(friend);
                _dc.SaveChanges();
            }
            if(request != null)
            {
                _dc.FriendRequests.Remove(request);
                _dc.SaveChanges();
            }
           
            return RedirectToAction("Index","Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveFriend(string id)
        {
            string sessionId = Session["Id"].ToString();
            var friend = _dc.Friends.Where(m => (m.Id_A == id && m.Id_F == sessionId) || (m.Id_F == id && m.Id_A == sessionId)).FirstOrDefault();

            _dc.Friends.Remove(friend);
            _dc.SaveChanges();
            

            return Json(true);
        }
        [HttpPost]
        public ActionResult DeleteAccount()
        {
            string id = Session["Id"].ToString();
            Account account = _dc.Accounts.Where(a => a.Id == id).FirstOrDefault();

            if (account != null)
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
            
            }
            return RedirectToAction("SignOut", "Account");
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