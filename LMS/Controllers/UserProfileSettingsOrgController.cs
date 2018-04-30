using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CLSLms;

namespace LMS.Controllers
{
    public class UserProfileSettingsOrgController : Controller
    {
        private LeopinkLMSDBEntities db = new LeopinkLMSDBEntities();

        // GET: /UserProfileSettingsOrg/
        public ActionResult Index()
        {
            var userprofilesettingsorgs = db.UserProfileSettingsOrgs.Include(u => u.Organisation).Include(u => u.UserProfileSetting);
            return View(userprofilesettingsorgs.ToList());
        }

        // GET: /UserProfileSettingsOrg/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfileSettingsOrg userprofilesettingsorg = db.UserProfileSettingsOrgs.Find(id);
            if (userprofilesettingsorg == null)
            {
                return HttpNotFound();
            }
            return View(userprofilesettingsorg);
        }

        // GET: /UserProfileSettingsOrg/Create
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.Organisations, "OrganisationID", "OrganisationID");
            ViewBag.ProfileSettingID = new SelectList(db.UserProfileSettings, "ProfileSettingID", "ProfileTitle");
            return View();
        }

        // POST: /UserProfileSettingsOrg/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ProfileSettignOrgId,ProfileSettingID,ID,IsDisplay,IsMandatory,MaxLength,ProfileType,LastModifiedById,CreatedOnDate")] UserProfileSettingsOrg userprofilesettingsorg)
        {
            if (ModelState.IsValid)
            {
                db.UserProfileSettingsOrgs.Add(userprofilesettingsorg);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.Organisations, "OrganisationID", "OrganisationID", userprofilesettingsorg.OrganisationID);
            ViewBag.ProfileSettingID = new SelectList(db.UserProfileSettings, "ProfileSettingID", "ProfileTitle", userprofilesettingsorg.ProfileSettingID);
            return View(userprofilesettingsorg);
        }

        // GET: /UserProfileSettingsOrg/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfileSettingsOrg userprofilesettingsorg = db.UserProfileSettingsOrgs.Find(id);
            if (userprofilesettingsorg == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID = new SelectList(db.Organisations, "OrganisationID", "OrganisationID", userprofilesettingsorg.OrganisationID);
            ViewBag.ProfileSettingID = new SelectList(db.UserProfileSettings, "ProfileSettingID", "ProfileTitle", userprofilesettingsorg.ProfileSettingID);
            return View(userprofilesettingsorg);
        }

        // POST: /UserProfileSettingsOrg/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ProfileSettignOrgId,ProfileSettingID,ID,IsDisplay,IsMandatory,MaxLength,ProfileType,LastModifiedById,CreatedOnDate")] UserProfileSettingsOrg userprofilesettingsorg)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userprofilesettingsorg).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID = new SelectList(db.Organisations, "OrganisationID", "OrganisationID", userprofilesettingsorg.OrganisationID);
            ViewBag.ProfileSettingID = new SelectList(db.UserProfileSettings, "ProfileSettingID", "ProfileTitle", userprofilesettingsorg.ProfileSettingID);
            return View(userprofilesettingsorg);
        }

        // GET: /UserProfileSettingsOrg/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfileSettingsOrg userprofilesettingsorg = db.UserProfileSettingsOrgs.Find(id);
            if (userprofilesettingsorg == null)
            {
                return HttpNotFound();
            }
            return View(userprofilesettingsorg);
        }

        // POST: /UserProfileSettingsOrg/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserProfileSettingsOrg userprofilesettingsorg = db.UserProfileSettingsOrgs.Find(id);
            db.UserProfileSettingsOrgs.Remove(userprofilesettingsorg);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
