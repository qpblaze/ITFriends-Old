using Konan.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime;
using Konan.Context;
using Konan.Models;

namespace Konan.Controllers
{
    [Authorize]
    [CustomAuthorizeAttribute]
    public class ProblemsController : Controller
    {
        KonanDBContext _dc = new KonanDBContext();
        public ActionResult Compiler()
        {
            return View();
        }

        public ActionResult Solve(string id)
        {

            Post post = _dc.Posts.Where(p => p.Id == id).FirstOrDefault();
            if(post != null)
            {
                return View(post);
            }

            return View();
        }

        [HttpPost]
        public JsonResult Solve(Solution model)
        {
            Solution sol = new Solution();
            sol = model;

            sol.Id_A = Session["Id"].ToString();
            sol.Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
            sol.Date = DateTime.Now;

            _dc.Solutions.Add(sol);
            _dc.SaveChanges();

            return Json(true);
        }


        public ActionResult Solution(string id)
        {
            Solution sol = _dc.Solutions.Where(s => s.Id == id).FirstOrDefault();
            if (sol != null)
            {
                return View(sol);
            }
            else
                return RedirectToAction("Compiler", "Problems");
        }
        public ActionResult GetSolutions(string id)
        {
            return PartialView("_SolutionsPartial", _dc.Solutions.Where(s => s.Id_P == id).OrderBy(s => s.Date).ToList());
        }

        public ActionResult GetProblems()
        {
            string id = Session["Id"].ToString();

            return PartialView("_ProblemsPartial", _dc.Posts.Where(p => p.Id_A == id && p.Output != null).OrderBy(p => p.Date).ToList());
        }


        private static async void MakeRequest(string code, string lang, string input)
        {
            

        }

        [HttpPost]
        public async Task<JsonResult> Compiler(string code, string language, string input)
        {
            if (language == "CPP")
            {
                language = "1";
            }

            if (language == "CSHARP")
            {
                language = "27";
            }

            if (language == "C")
            {
                language = "11";
            }

            if (language == "CLOJURE")
            {
                language = "111";
            }

            if (language == "JAVA")
            {
                language = "10";
            }

            if (language == "JAVASCRIPT")
            {
                language = "35";
            }

            if (language == "HASKELL")
            {
                language = "21";
            }

            if (language == "PERL")
            {
                language = "54";
            }

            if (language == "PHP")
            {
                language = "29";
            }

            if (language == "PYTHON")
            {
                language = "99";
            }

            if (language == "RUBY")
            {
                language = "17";
            }


            code = code.Replace("&gt;", ">").Replace("&lt;", "<");
            try
            {
                string token = "2e899925328c3e5340a0ed23af34ad30";

                HttpClient client = new HttpClient();

                string postData = "access_token=" + token
                                  + "&sourceCode=" + Uri.EscapeDataString(code)
                                  + "&language=" + language
                                  + "&input=" + Uri.EscapeDataString(input);

                string url = "http://e855a150.compilers.sphere-engine.com/api/v3/submissions/?" + postData;

                var response = await client.PostAsync(url, null);


                var submissionId = await response.Content.ReadAsStringAsync();

                int pFrom = submissionId.IndexOf("id\":\"") + "id\":\"".Length;
                int pTo = submissionId.LastIndexOf("\"}");

                submissionId = submissionId.Substring(pFrom, pTo - pFrom);
                //Debug.WriteLine("result: " + submissionId);


                url = "http://e855a150.compilers.sphere-engine.com/api/v3/submissions/" + submissionId + "?access_token=2e899925328c3e5340a0ed23af34ad30&withSource=1&withInput=1&withOutput=1&withStderr=1&withCmpinfo=1";

                RunStatus rs = new RunStatus();

                int tried = 0;
                var result = "";
                while (string.IsNullOrEmpty(rs.output) && string.IsNullOrEmpty(rs.cmpinfo) && string.IsNullOrEmpty(rs.stderr))
                {
                    await Task.Delay(1000);
                    response = await client.GetAsync(url);
                    result = await response.Content.ReadAsStringAsync();
                    rs = new JavaScriptSerializer().Deserialize<RunStatus>(result);
                    tried++;

                    if (tried == 6)
                    {
                        rs.output = "no output...";
                        return Json(rs);
                    }
                }

                

               // Debug.WriteLine("\n-------------------------------------------\n" + result);
               // Debug.WriteLine("\n" + rs.output);

                return Json(rs);


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
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