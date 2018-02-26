using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CLSLms;
using LMS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using System.Globalization;

namespace LMS.Controllers
{
    public class HomeController : Controller
    {
        private LeopinkLMSDBEntities db = new LeopinkLMSDBEntities();
        public HomeController() : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }
        public ActionResult Index()
        {

            var currentUser = User.Identity.GetUserId();
            if (currentUser == null || Session["MAclActions"] == null)
            {
                return RedirectToAction("login", "Account");
            }
            else
            {
                if (Session["CourseMangerRole"].ToString().ToLower() != "na")
                {
                    return RedirectToAction("Index", "Course");
                }
                else if (Session["IsAdminView"].ToString().ToLower() == "true")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (Session["IsParentTeacher"].ToString().ToLower() == "true")
                {
                    return RedirectToAction("Index", "ParentTeacher");
                }
                else
                {
                    return RedirectToAction("Index", "UserCourse");
                }
            }
            //var model = new GroupLocalView();           
            //return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            SupportInfo supportinfo = new SupportInfo();
            long uid = Convert.ToInt64(Session["UserID"]);
            var objuser = db.UserProfiles.Where(usr => usr.UserId == uid).FirstOrDefault();
            if (objuser != null)
            {
                ViewBag.UserName = objuser.FirstName + ' ' + objuser.LastName;
                ViewBag.UserEmail = objuser.EmailAddress;
            }
            return View(supportinfo);
        }
        [HttpPost]
        public ActionResult Contact(SupportInfo supportinfo)
        {
            bool SupportFileExist = false;
            string attachfilepath = "";
            string fPath = "";
            if (Request.Files.Count > 0)
            {
                foreach (string file in Request.Files)
                {
                    if (file.ToLower() == "supportfilepath")
                    {
                        HttpPostedFileBase hpf = Request.Files[file];
                        if (hpf.FileName != "")
                        {
                            string extension = System.IO.Path.GetExtension(hpf.FileName);
                            string fname = "";
                            do
                            {
                                fname = Guid.NewGuid().ToString();
                            } while (!Common.IsValidFileName(Guid.NewGuid().ToString(), true));

                            string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads/Contents/Support"), fname + extension);
                            hpf.SaveAs(path1);
                            attachfilepath = path1;
                            fPath = "/Content/Uploads/Contents/Support/" + fname + extension;
                            SupportFileExist = true;
                        }
                    }
                }
            }

            // save the SupportInfo object in database.
            SupportInfo ObjSupport = new SupportInfo(); // create object of SupportInfo table to save the record
            ObjSupport.Summary = supportinfo.Summary;
            ObjSupport.Description = supportinfo.Description;
            if (SupportFileExist == true)
                ObjSupport.FilePath = fPath;
            ObjSupport.UserID = Convert.ToInt64(Session["UserID"]);
            ObjSupport.SupportDate = DateTime.Now;
            db.SupportInfoes.Add(ObjSupport);
            db.SaveChanges(); // save data in SupportInfo table in database

            string useremail = "";
            long uid = Convert.ToInt64(Session["UserID"]);
            var objuser = db.UserProfiles.Where(usr => usr.UserId == uid).FirstOrDefault();
            if (objuser != null)
            {
                useremail = objuser.EmailAddress;
            }
            var objInstance = (from o in db.InstanceInfoes
                               where o.InstanceID == 1
                               select new { o.InstanceTitle, o.HostEmail, o.URL, o.SmtpIPv4 }).FirstOrDefault();
            if (objInstance != null && useremail != "")
            {
                string fromEmail = useremail;

                string subject = supportinfo.Summary;

                string body = "<div style='font-family: Arial; font-size: 10pt'>Date sumbitted: " + String.Format("{0:d/M/yyyy}", DateTime.Now) + "<br />Subject: " + supportinfo.Summary + "<br /> Detail: " + supportinfo.Description + "</div>";

                //body = body.Replace("{InstanceTitle}", objInstance.InstanceTitle).Replace("{InstanceURL}", objInstance.URL).Replace("{FirstName}", HttpUtility.HtmlEncode(objUser.FirstName)).Replace("{UserName}", HttpUtility.HtmlEncode(objUser.EmailAddress)).Replace("{Password}", Password).Replace("{Password}", Password); //edit it
                try
                {
                    if (SupportFileExist == true)
                        MailEngine.SendWithAttachment(fromEmail, objInstance.HostEmail, subject, body, objInstance.SmtpIPv4, attachfilepath);
                    else
                        MailEngine.Send(fromEmail, objInstance.HostEmail, subject, body, objInstance.SmtpIPv4);

                    //MailEngine oLog = new MailEngine();
                    //oLog.LogEmail(fromEmail, objEmail.ID, objUser.UserId);
                    //db.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
            //string urlref = Request.UrlReferrer.AbsolutePath;
            //if (urlref.Contains("/") == true)
            //{
            //    string cont = urlref.Substring(urlref.LastIndexOf("/") + 1);
            //    if (cont.ToLower() == "edituserprofile")
            //        return RedirectToAction(cont, "User");
            //    else
            //        return RedirectToAction("Index", cont);
            //}
            return RedirectToAction("Index", "User");
        }
        public ActionResult Support()
        {
            SupportInfo supportinfo = new SupportInfo();
            long uid = Convert.ToInt64(Session["UserID"]);
            var objuser = db.UserProfiles.Where(usr => usr.UserId == uid).FirstOrDefault();
            if (objuser != null)
            {
                ViewBag.UserName = objuser.FirstName + ' ' + objuser.LastName;
                ViewBag.UserEmail = objuser.EmailAddress;
            }

            return PartialView("_Support", supportinfo);
        }

        [HttpPost]
        public ActionResult UserSupport(SupportInfo supportinfo)
        {
            bool SupportFileExist = false;
            string attachfilepath = "";
            string fPath = "";
            if (Request.Files.Count > 0)
            {
                foreach (string file in Request.Files)
                {
                    if (file.ToLower() == "supportfilepath")
                    {
                        HttpPostedFileBase hpf = Request.Files[file];
                        if (hpf.FileName != "")
                        {
                            string extension = System.IO.Path.GetExtension(hpf.FileName);
                            string fname = "";
                            do
                            {
                                fname = Guid.NewGuid().ToString();
                            } while (!Common.IsValidFileName(Guid.NewGuid().ToString(), true));

                            string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads/Contents/Support"), fname + extension);
                            hpf.SaveAs(path1);
                            attachfilepath = path1;
                            fPath = "/Content/Uploads/Contents/Support/" + fname + extension;
                            SupportFileExist = true;
                        }
                    }
                }
            }

            // save the SupportInfo object in database.
            SupportInfo ObjSupport = new SupportInfo(); // create object of SupportInfo table to save the record
            ObjSupport.Summary = supportinfo.Summary;
            ObjSupport.Description = supportinfo.Description;
            if (SupportFileExist == true)
                ObjSupport.FilePath = fPath;
            ObjSupport.UserID = Convert.ToInt64(Session["UserID"]);
            ObjSupport.SupportDate = DateTime.Now;
            db.SupportInfoes.Add(ObjSupport);
            db.SaveChanges(); // save data in SupportInfo table in database

            string useremail = "";
            long uid = Convert.ToInt64(Session["UserID"]);
            var objuser = db.UserProfiles.Where(usr => usr.UserId == uid).FirstOrDefault();
            if (objuser != null)
            {
                useremail = objuser.EmailAddress;
            }
            var objInstance = (from o in db.InstanceInfoes
                               where o.InstanceID == 1
                               select new { o.InstanceTitle, o.HostEmail, o.URL, o.SmtpIPv4 }).FirstOrDefault();
            if (objInstance != null && useremail != "")
            {
                string fromEmail = useremail;

                string subject = supportinfo.Summary;

                string body = "<div style='font-family: Arial; font-size: 10pt'>Date sumbitted: " + String.Format("{0:d/M/yyyy}", DateTime.Now) + "<br />Subject: " + supportinfo.Summary + "<br /> Detail: " + supportinfo.Description + "</div>";

                //body = body.Replace("{InstanceTitle}", objInstance.InstanceTitle).Replace("{InstanceURL}", objInstance.URL).Replace("{FirstName}", HttpUtility.HtmlEncode(objUser.FirstName)).Replace("{UserName}", HttpUtility.HtmlEncode(objUser.EmailAddress)).Replace("{Password}", Password).Replace("{Password}", Password); //edit it
                try
                {
                    if (SupportFileExist == true)
                        MailEngine.SendWithAttachment(fromEmail, objInstance.HostEmail, subject, body, objInstance.SmtpIPv4, attachfilepath);
                    else
                        MailEngine.Send(fromEmail, objInstance.HostEmail, subject, body, objInstance.SmtpIPv4);

                    //MailEngine oLog = new MailEngine();
                    //oLog.LogEmail(fromEmail, objEmail.ID, objUser.UserId);
                    //db.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
            string urlref = Request.UrlReferrer.AbsolutePath;
            if (urlref.Contains("/") == true)
            {
                string cont = urlref.Substring(urlref.LastIndexOf("/") + 1);
                if (cont.ToLower() == "edituserprofile")
                    return RedirectToAction(cont, "User");
                else
                    return RedirectToAction("Index", cont);
            }
            return RedirectToAction("Index");
        }

        public ActionResult DisplayHelp()
        {
            InstanceInfo instanceinfo = db.InstanceInfoes.Find(1);
            string url = instanceinfo.URL;

            HelpModules helpmodule = new HelpModules();
            string urlref = Request.UrlReferrer.ToString();

            if (urlref.Contains("/") == true)
            {
                string cont = urlref.Substring(urlref.LastIndexOf("/") + 1);
                if (Common.IsNumeric(cont) == true)
                {
                    urlref = urlref.Substring(url.Length + 1);
                    urlref = urlref.Substring(0, urlref.Length - cont.ToString().Length - 1);
                }
                else
                {
                    urlref = urlref.Substring(url.Length + 1);
                }

                var objhelpcontent = db.HelpContents.Where(hc => hc.ModuleSectionURL == urlref).FirstOrDefault();
                if (objhelpcontent != null)
                {
                    helpmodule.ModuleTitle = objhelpcontent.ModuleTitle;
                    helpmodule.ModuleContent = objhelpcontent.ModuleContent;
                }
                else
                {
                    helpmodule.ModuleTitle = "No Help";
                    helpmodule.ModuleContent = "Help Not exists for this module";
                }
            }
            else
            {
                helpmodule.ModuleTitle = "No Help";
                helpmodule.ModuleContent = "Help Not exists for this module";
            }
            return PartialView("_Help", helpmodule);
        }

        public void uploadnow(HttpPostedFileWrapper upload)
        {
            if (upload != null)
            {
                var objInstance = (from o in db.InstanceInfoes
                                   where o.InstanceID == 1
                                   select new { o.URL }).FirstOrDefault();
                if (objInstance != null)
                {
                    string imgurl = objInstance.URL;
                    string ImageName = upload.FileName;
                    imgurl = imgurl + "/Content/Uploads/Help/" + ImageName;
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploads/Help"), ImageName);
                    upload.SaveAs(path);
                    HttpContext.Response.Write("<script>window.parent.CKEDITOR.tools.callFunction(" + 1 + ", \"" + imgurl + "\");</script>");
                }
            }
        }

        public ActionResult uploadPartial()
        {
            var appData = Server.MapPath("~/Content/Uploads/Help");
            var images = Directory.GetFiles(appData).Select(x => new imagesviewmodel
            {
                Url = Url.Content("~/Content/Uploads/Help/" + Path.GetFileName(x))
            });
            return View(images);
        }
    }
}