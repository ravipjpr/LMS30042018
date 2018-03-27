using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CLSLms;
using LMS.Models;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using System.Globalization;

namespace LMS.Controllers
{
    [CustomAuthorize]
    public class OrganisationController : Controller
    {
        
        private LeopinkLMSDBEntities db = new LeopinkLMSDBEntities();

        #region// Organisation List

        public ActionResult Index()
        {
            return View();
        }
        //
        // GET: /Organisation List
        
        public ActionResult AjaxHandlerOrganisation(jQueryDataTableParamModel param)
        {
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<Organisation, string> orderingFunction = (c => sortColumnIndex == 0 ? string.IsNullOrWhiteSpace(c.OrganisationUID) ? "" : c.OrganisationUID.ToLower() :
                                                        sortColumnIndex == 1 ? c.OrganisationName.ToLower() :                                                        
                                                        sortColumnIndex == 2 ? string.IsNullOrWhiteSpace(c.AddressLine1) ? "" : c.AddressLine1.ToLower() :
                                                        sortColumnIndex == 3 ? string.IsNullOrWhiteSpace(c.AddressLine2) ? "" : c.AddressLine2.ToLower() :
                                                        sortColumnIndex == 4 ? string.IsNullOrWhiteSpace(c.Country) ? "" : c.Country.ToLower() :
                                                        sortColumnIndex == 5 ? Convert.ToString(c.Status.ToString()):                                                        
                                                        c.OrganisationName.ToLower());
             var sortDirection = Request["sSortDir_0"];
            IEnumerable<Organisation> filterOrganisation = null;

            

            /// search action
             if (!string.IsNullOrEmpty(param.sSearch))
            {
                
                     if(sortDirection == "asc")
                     {
                         filterOrganisation = from or in db.Organisations
                                              where or.OrganisationName.ToLower().Contains(param.sSearch.ToLower())
                                              select or;
                     }
                     else
                     {
                         filterOrganisation = from or in db.Organisations
                                              where or.OrganisationName.ToLower().Contains(param.sSearch.ToLower())
                                              select or;
                     }
                 
             }
             else                 
             {
                 filterOrganisation = from or in db.Organisations
                                      select or;
             }

            // ordering action
             if (sortColumnIndex == 7)
             {
                 if(sortDirection == "asc")
                 { filterOrganisation = filterOrganisation.OrderBy(x => x.CreationDate); }
                 else
                 { filterOrganisation = filterOrganisation.OrderByDescending(x => x.CreationDate); }
             }
             else  if (sortDirection == "asc")
                {
                    filterOrganisation = filterOrganisation.OrderBy(orderingFunction);
                }
                else if (sortDirection == "desc")
                {
                    filterOrganisation = filterOrganisation.OrderByDescending(orderingFunction);
                }

            filterOrganisation = filterOrganisation.Where(x=>x.IsDeleted == false).ToList();

            // records to display            
            var displayedOrganisation = filterOrganisation.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            if (param.iDisplayLength == -1)
                displayedOrganisation = filterOrganisation;
            var ActiveStatus = LMSResourse.Common.Common.lblActiveStatus;
            var InactiveStatus = LMSResourse.Common.Common.lblInactiveStatus;
            var result = from obj in displayedOrganisation.ToList()
                         
                          select new[] {
                              Convert.ToString(obj.OrganisationUID),
                              obj.OrganisationName,                              
                              string.IsNullOrWhiteSpace(obj.AddressLine1)?"":obj.AddressLine1,
                               string.IsNullOrWhiteSpace(obj.AddressLine2)?"":obj.AddressLine2,
                               string.IsNullOrWhiteSpace(obj.Country)?"":obj.Country,
                               (obj.Status)?ActiveStatus :InactiveStatus,
                               (obj.Groups.Count()>0)?JsonConvert.SerializeObject(obj.Groups.DefaultIfEmpty().Where(x=>x.IsDeleted == false).Select(x=>new string[]{x.GroupName}).ToArray()).Replace("[[\"","").Replace("\"]]","").Replace("\"],",", ").Replace("[\"",""):"",
                              //string.Format("{0:dd/MM/yyyy HH:mm}",obj.CreationDate),
                              Convert.ToString(obj.OrganisationID)
                              
                          };

             return Json(new
                            {
                                sEcho = param.sEcho,
                                iTotalRecords = filterOrganisation.Count(),
                                iTotalDisplayRecords = filterOrganisation.Count(),
                                aaData = result
                            },
                            JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region // Create Organization

        [CustomAuthorize]
        public ActionResult CreateOrganisation()
        {
            var OrganisationModel = new OrganisationCreate();

            var model = new GroupLocalView();

            var AssignTypelist = new List<SelectListItem>();
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblGroupAssignment, Value = "false" });
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblIndividualAssignment, Value = "true" });
            ViewBag.CourseAssignTypelist = AssignTypelist;

            var selectedGroupsLocal = new List<GroupLocal>();
            selectedGroupsLocal.Add(new GroupLocal { ID = 0, GroupName = "-Selected-", IsSelected = true });
            model.AvailableGroups = db.Groups.Where(grp => grp.IsDeleted == false && grp.Status == true && grp.Organisation == null).Select(grp => new GroupLocal { ID = grp.GroupID, GroupName = grp.GroupName, IsSelected = false }).ToList();
            model.SelectedGroups = selectedGroupsLocal;
            OrganisationModel.GroupListView = model;
            OrganisationModel.Status = true;
            OrganisationModel.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.
            return View(OrganisationModel);
        }
        /// <summary>
        /// http post method for create organisation
        /// </summary>
        /// <param name="orgObj"></param>
        /// <returns></returns>
        [CustomAuthorize]
        [HttpPost]
        public ActionResult CreateOrganisation(OrganisationCreate orgObj,FormCollection coll)
        {
            string[] AllowedFileExtensions = new string[] { ".png",".gif",".jpg",".jpeg" }; // file extention of organisation logo and banner file.
            var OrganisationLogo = "";
            var OrganisationBanner = "";
            //bool OrganisationLogoExist = false;
            //bool OrganisationBannerExist = false;
            string fileName = "";
            //int[] ia = {};
            //if(orgObj.GroupListView!=null)
            //    if(orgObj.GroupListView.PostedGroups != null)
            //        if(orgObj.GroupListView.PostedGroups.GroupLocalIds.Length >0)
            //ia = orgObj.GroupListView.PostedGroups.GroupLocalIds.Select(n => Convert.ToInt32(n)).ToArray();

            // check the duplicate name for organisation.
            var chkduplicate = from org in db.Organisations
                               where (org.OrganisationUID.ToLower().Trim() == orgObj.OrganisationUID.ToLower().Trim() ||
                               org.OrganisationName.ToLower().Trim() == orgObj.OrganisationName.ToLower().Trim()) && (org.IsDeleted == false) 
                               select org;
            //var assignment = coll["assignment"] != null ? coll["assignment"].ToString() : "group";
            var AssignTypelist = new List<SelectListItem>();
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblGroupAssignment, Value = "false", Selected = (orgObj.IsUserAssignment == false) });
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblIndividualAssignment, Value = "true", Selected = (orgObj.IsUserAssignment == true)});
            ViewBag.CourseAssignTypelist = AssignTypelist;
            //Boolean isIndividual = assignment == "group" ? false : true;
                if (chkduplicate.Count() == 0)
                {

                        #region  // Uploading Organization Logo & Banner
                        if (Request.Files.Count > 0)
                        {
                            foreach (string file in Request.Files)
                            {
                                //if (file.ToLower() == "organisationlogo")
                                //{ OrganisationLogoExist = true; }
                                //if (file.ToLower() == "organisationbanner")
                                //{ OrganisationBannerExist = true; }

                                HttpPostedFileBase hpf = Request.Files[file];
                                if (hpf.FileName != "")
                                {
                                    fileName = hpf.FileName.Split('\\').Last().ToString();
                                    if (Common.IsValidFileName(hpf.FileName.Split('\\').Last().ToString(), true) == false) // check the valid file name 
                                    {
                                        if (file.ToLower() == "organisationlogo")
                                            ModelState.AddModelError("organisationlogo", "");
                                        //else if (file.ToLower() == "organisationbanner")
                                        //    ModelState.AddModelError("organisationbanner", "");
                                        return View(orgObj);
                                    }
                                    if (!AllowedFileExtensions.Contains(fileName.Substring(fileName.LastIndexOf('.')).ToLower())) // check the valid file extention
                                    {
                                        if (file.ToLower() == "organisationlogofile")
                                            ModelState.AddModelError("organisationlogo", "");
                                        //else if (file.ToLower() == "organisationbannerfile")
                                        //    ModelState.AddModelError("organisationbanner", "");
                                        return View(orgObj);
                                    }

                                    Guid g = Guid.NewGuid();

                                    var absImgDir = Path.Combine(Server.MapPath("~") + @"\Content\Uploads\Organisation\", "_" + g.ToString());
                                    Directory.CreateDirectory(absImgDir);
                                    var relCourseDir = Path.Combine(@"\Content\Uploads\Organisation\", "_" + g.ToString());
                                    hpf.SaveAs(Path.Combine(absImgDir, @fileName)); // save the files

                                    if (file.ToLower() == "organisationlogofile")
                                        OrganisationLogo = Path.Combine(relCourseDir, @fileName);
                                    else if (file.ToLower() == "organisationbannerfile")
                                        OrganisationBanner = Path.Combine(relCourseDir, @fileName);
                                }
                            }
                        }
                        #endregion

                        #region Declaring Organization object Model & Initializing and creating organization
                        Organisation orgNew = new Organisation(); // create organisation object to update data in database.
                        orgNew.OrganisationUID = orgObj.OrganisationUID;
                        orgNew.OrganisationName = orgObj.OrganisationName;
                        orgNew.AddressLine1 = orgObj.AddressLine1;
                        orgNew.AddressLine2 = orgObj.AddressLine2;
                        orgNew.AddressLine3 = orgObj.AddressLine3;
                        orgNew.PostalCode = orgObj.PostalCode;
                        orgNew.Country = orgObj.Country;
                        orgNew.Status = orgObj.Status;
                        orgNew.CreatedById = Convert.ToInt64(Session["UserID"]);
                        orgNew.CreationDate = DateTime.Now;
                        orgNew.IsDeleted = false;
                        orgNew.OrganisationLogo = OrganisationLogo;
                        orgNew.OrganisationBanner = OrganisationBanner;
                        orgNew.MaxUsers = orgObj.MaxUsers;
                        orgNew.IsUserAssignment = orgObj.IsUserAssignment;
                        orgNew.ExpiryDate = DateTime.ParseExact(orgObj.ExpDate + " 23:59:59", ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(), CultureInfo.InvariantCulture);
                        db.Organisations.Add(orgNew);
                        db.SaveChanges(); // save organistion object in database
                        #endregion

                        #region Declaring OrganisationInfo object Model & Initializing and creating OrganisationInfo
                        OrganisationInfo orgInfo = new OrganisationInfo(); // create organistion info object to update date in database.
                        orgInfo.OrganisationID = orgNew.OrganisationID;
                        orgInfo.OrganisationUID = orgNew.OrganisationUID;
                        orgInfo.OrganisationName = orgObj.OrganisationName;
                        orgInfo.AddressLine1 = orgObj.AddressLine1;
                        orgInfo.AddressLine2 = orgObj.AddressLine2;
                        orgInfo.AddressLine3 = orgObj.AddressLine3;
                        orgInfo.PostalCode = orgObj.PostalCode;
                        orgInfo.Country = orgObj.Country;
                        orgInfo.OrganisationLogo = OrganisationLogo;
                        orgInfo.OrganisationBanner = OrganisationBanner;
                        orgInfo.LanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;
                        orgInfo.CreatedById = Convert.ToInt64(Session["UserID"]);
                        orgInfo.CreationDate = DateTime.Now;
                        db.OrganisationInfoes.Add(orgInfo);
                        db.SaveChanges(); // save organisation info object in database i.e is organisation data depending on language.
                        #endregion

                        var _OID = orgNew.OrganisationID;
                //using (db) // set the group organisation relation ship in database by updating the organisation field of group table.
                //{
                //    var some = db.Groups.Where(x => ia.Contains(x.GroupID) && x.Status == true && x.Organisation == null).ToList();
                //    some.ForEach(a => a.OrganisationID = orgNew.OrganisationID);
                //    db.SaveChanges();
                //}


                Group grp = new Group();
                grp.Status = true;
                grp.GroupName = orgNew.OrganisationName;
                grp.GroupDescription = orgNew.OrganisationName;
                grp.OrganisationID = orgNew.OrganisationID;
                grp.MaxUsers = orgNew.MaxUsers;
                grp.CreationDate = DateTime.Now;
                grp.CreatedById = Convert.ToInt64(Session["UserID"]);
                grp.IsDeleted = false;
                db.Groups.Add(grp);
                db.SaveChanges(); // save th group data in database

                                  //foreach (var _gid in ia)
                                  //{
                                  //    var Newgroups = db.Groups.Where(x => x.GroupID == _gid && x.Status == true && x.Organisation == null).FirstOrDefault();
                                  //    if (Newgroups != null)
                                  //    {
                                  //        Newgroups.OrganisationID = _OID;
                                  //        Newgroups.MaxUsers = orgNew.MaxUsers;
                                  //        db.SaveChanges();
                                  //    }
                                  //}

                #region creating default mails for new organization
                string[] EmailCodes = new string[] { "REGU", "REGA", "FPASS", "GNPASS","CGASS" };

                        foreach (var mailcode in EmailCodes)
                        {
                            var objEmail = db.Emails.FirstOrDefault(e => e.MailCode == mailcode && e.OrganisationID == 0);
                            if (objEmail != null)
                            {
                                Email objEmailConfig = new Email(); // create Email object to update date in database.
                                objEmailConfig.Subject = objEmail.Subject;
                                objEmailConfig.IsOn = objEmail.IsOn;
                                objEmailConfig.Body = objEmail.Body;
                                objEmailConfig.Name = objEmail.Name;
                                objEmailConfig.Mailtype = objEmail.Mailtype;
                                objEmailConfig.MailCode = objEmail.MailCode;
                                objEmailConfig.OrganisationID = _OID;
                                objEmailConfig.IsOn = objEmail.IsOn;
                                objEmailConfig.IsDefault = objEmail.IsDefault;
                                objEmailConfig.FromEmail = objEmail.FromEmail;
                                objEmailConfig.LastModifiedByID = Convert.ToInt64(Session["UserID"]);
                                objEmailConfig.DateLastModified = DateTime.Now;
                                db.Emails.Add(objEmailConfig);
                                db.SaveChanges(); // save Email object in database i.e create default mails for new organisation
                            }
                        }
                        #endregion
                        return RedirectToAction("Index", "Organisation");

                }
                else
                {
                    if (chkduplicate.Where(x => x.OrganisationUID.ToLower().Trim() == orgObj.OrganisationUID.ToLower().Trim()).Select(x => x).Count() > 0)
                        ModelState.AddModelError("OrganisationUID", LMSResourse.Admin.Organisation.msgDupOrganisationID);
                    if (chkduplicate.Where(x => x.OrganisationName.ToLower().Trim() == orgObj.OrganisationName.ToLower().Trim()).Select(x => x).Count() > 0)
                        ModelState.AddModelError("OrganisationName", LMSResourse.Admin.Organisation.msgDupOrganisationName);
                }
            
            var model = new GroupLocalView();
            var selectedGroupsLocal = new List<GroupLocal>();
            selectedGroupsLocal.Add(new GroupLocal { ID = 0, GroupName = "-Selected-", IsSelected = true });
            //model.AvailableGroups = db.Groups.Where(grp => grp.IsDeleted == false && grp.Status == true && grp.Organisation == null).Select(grp => new GroupLocal { ID = grp.GroupID, GroupName = grp.GroupName, IsSelected = (ia.Contains(grp.GroupID) ? true : false) }).ToList();
            model.SelectedGroups = selectedGroupsLocal;
            orgObj.GroupListView = model;  
            return View(orgObj);
        }
        #endregion


       
        #region // Edit Organisation
        /// <summary>
        /// http get method of edit organisation.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CustomAuthorize]
        public ActionResult EditOrganisation(int id =0)
        {
            var ObjOrganisation = db.Organisations.Find(id);
            if (ObjOrganisation == null)
            {
                return RedirectToAction("Index", "Organisation");
            }
            else if(ObjOrganisation.IsDeleted == true)
                return RedirectToAction("Index", "Organisation");

            var AssignTypelist = new List<SelectListItem>();
            // Set the course window type to fill in dropdown list on page.
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblGroupAssignment, Value = "false", Selected = (ObjOrganisation.IsUserAssignment == false) ? true : false });
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblIndividualAssignment, Value = "true", Selected = (ObjOrganisation.IsUserAssignment == true) ? true : false });
            ViewBag.CourseAssignTypelist = AssignTypelist;

            // if organisation exist and not deleted then create the object of 
            // organistioncreate model the will return to view. // object value are assigned from existing organisation data.
            var OrganisationModel = new OrganisationCreate();
            OrganisationModel.OrganisationID = ObjOrganisation.OrganisationID;
            OrganisationModel.OrganisationUID = ObjOrganisation.OrganisationUID;
            OrganisationModel.OrganisationName = ObjOrganisation.OrganisationName;
            OrganisationModel.AddressLine1 = ObjOrganisation.AddressLine1;
            OrganisationModel.AddressLine2 = ObjOrganisation.AddressLine2;
            OrganisationModel.AddressLine3 = ObjOrganisation.AddressLine3;
            OrganisationModel.PostalCode = ObjOrganisation.PostalCode;
            OrganisationModel.Status = ObjOrganisation.Status;
            OrganisationModel.Groups = OrganisationModel.Groups;
            OrganisationModel.Country = ObjOrganisation.Country;
            OrganisationModel.OrganisationBanner = ObjOrganisation.OrganisationBanner;
            OrganisationModel.OrganisationLogo = ObjOrganisation.OrganisationLogo;
            OrganisationModel.MaxUsers = ObjOrganisation.MaxUsers;
            OrganisationModel.IsUserAssignment = ObjOrganisation.IsUserAssignment;
            OrganisationModel.ExpiryDate = ObjOrganisation.ExpiryDate;
            OrganisationModel.ExpDate = ObjOrganisation.ExpiryDate.Value.ToString(ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(),CultureInfo.InvariantCulture).Substring(0,10);
            //OrganisationModel.ExpDate = ObjOrganisation.ExpiryDate.HasValue && ObjOrganisation.ExpiryDate.Value.ToString() != "" ? ObjOrganisation.ExpiryDate.Value.ToString() : "";
            OrganisationModel.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.
            var maxAssignedUser = db.Groups.Where(a => a.OrganisationID == ObjOrganisation.OrganisationID).Select(a => a.MaxUsers).Max();
            ViewBag._maxUserAssignrdToGroup = maxAssignedUser;
            var model = new GroupLocalView();
            // set the selected and available group when the organisation is edited. Available groups are "Assigned group to organisation" + "groups that are not linked to any organisation"
            var selectedGroupsLocal = ObjOrganisation.Groups.Select(grp => new GroupLocal { ID = grp.GroupID, GroupName = grp.GroupName, IsSelected = true }).ToList();
            model.AvailableGroups = db.Groups.Where(grp => grp.IsDeleted == false && grp.Status == true && (grp.Organisation == null || grp.OrganisationID == ObjOrganisation.OrganisationID)).Select(grp => new GroupLocal { ID = grp.GroupID, GroupName = grp.GroupName, IsSelected = (grp.Organisation == null ? false : true) }).ToList();
            model.SelectedGroups = selectedGroupsLocal;
            OrganisationModel.GroupListView = model;

            

            return View(OrganisationModel);
        }

        /// <summary>
        /// http post method for edit organisation.
        /// </summary>
        /// <param name="orgObj"></param>
        /// <returns></returns>
        [CustomAuthorize]
        [HttpPost]
        public ActionResult EditOrganisation(OrganisationCreate orgObj,FormCollection coll)
        {
            string[] AllowedFileExtensions = new string[] { ".png", ".gif", ".jpg", ".jpeg" };
            //var OrganisationLogoExist = false;
            //var OrganisationBannerExist = false;
            var OrganisationLogo = "";
            var OrganisationBanner = "";
            string fileName = "";
            //var assignment = coll["assignment"] != null ? coll["assignment"].ToString() : "group";
            //Boolean isIndividual = assignment == "group" ? false : true;

            var AssignTypelist = new List<SelectListItem>();
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblGroupAssignment, Value = "false", Selected = (orgObj.IsUserAssignment == false) ? true : false });
            AssignTypelist.Add(new SelectListItem() { Text = LMSResourse.Admin.Organisation.lblIndividualAssignment, Value = "true", Selected = (orgObj.IsUserAssignment == true) ? true : false });
            ViewBag.CourseAssignTypelist = AssignTypelist;

            var chkObjExistTemp = from org in db.Organisations
                               where (org.OrganisationUID.ToLower().Trim() == orgObj.OrganisationUID.ToLower().Trim() ||
                               org.OrganisationName.ToLower().Trim() == orgObj.OrganisationName.ToLower().Trim()) && org.OrganisationID != orgObj.OrganisationID && org.IsDeleted == false
                               select org;
            orgObj.ExpDate = orgObj.ExpDate;
            int[] ia = { };
            if (orgObj.GroupListView != null)
                if (orgObj.GroupListView.PostedGroups != null)
                    if (orgObj.GroupListView.PostedGroups.GroupLocalIds.Length > 0)
                        ia = orgObj.GroupListView.PostedGroups.GroupLocalIds.Select(n => Convert.ToInt32(n)).ToArray();

            var chkObjExist = db.Organisations.Find(orgObj.OrganisationID);
                               
                if (chkObjExistTemp.Count() == 0)
                {
                    if (ia.Length > 0)
                    {
                        #region Uploading Region Organization Logo & Banner
                        if (Request.Files.Count > 0)
                        {
                            foreach (string file in Request.Files)
                            {
                                //if (file.ToLower() == "organisationlogo")
                                //{ OrganisationLogoExist = true; }
                                //if (file.ToLower() == "organisationbanner")
                                //{ OrganisationBannerExist = true; }

                                HttpPostedFileBase hpf = Request.Files[file];
                                if (hpf.FileName != "")
                                {
                                    fileName = hpf.FileName.Split('\\').Last().ToString();
                                    if (Common.IsValidFileName(hpf.FileName.Split('\\').Last().ToString(), true) == false)
                                    {
                                        if (file.ToLower() == "organisationlogo")
                                            ModelState.AddModelError("organisationlogo", "");
                                        //else if (file.ToLower() == "organisationbanner")
                                        //    ModelState.AddModelError("organisationbanner", "");
                                        return View(orgObj);
                                    }
                                    if (!AllowedFileExtensions.Contains(fileName.Substring(fileName.LastIndexOf('.')).ToLower()))
                                    {
                                        if (file.ToLower() == "organisationlogofile")
                                            ModelState.AddModelError("organisationlogo", "");
                                        //else if (file.ToLower() == "organisationbannerfile")
                                        //    ModelState.AddModelError("organisationbanner", "");
                                        return View(orgObj);
                                    }

                                    Guid g = Guid.NewGuid();

                                    var absImgDir = Path.Combine(Server.MapPath("~") + @"\Content\Uploads\Organisation\", "_" + g.ToString());
                                    Directory.CreateDirectory(absImgDir);
                                    var relCourseDir = Path.Combine(@"\Content\Uploads\Organisation\", "_" + g.ToString());
                                    hpf.SaveAs(Path.Combine(absImgDir, @fileName));

                                    if (file.ToLower() == "organisationlogofile")
                                        OrganisationLogo = Path.Combine(relCourseDir, @fileName);
                                    else if (file.ToLower() == "organisationbannerfile")
                                        OrganisationBanner = Path.Combine(relCourseDir, @fileName);
                                }
                            }
                        }
                        #endregion

                        chkObjExist.OrganisationUID = orgObj.OrganisationUID;
                        chkObjExist.OrganisationName = orgObj.OrganisationName;
                        chkObjExist.AddressLine1 = orgObj.AddressLine1;
                        chkObjExist.AddressLine2 = orgObj.AddressLine2;
                        chkObjExist.AddressLine3 = orgObj.AddressLine3;
                        chkObjExist.PostalCode = orgObj.PostalCode;
                        chkObjExist.Country = orgObj.Country;
                        chkObjExist.OrganisationLogo = string.IsNullOrWhiteSpace(OrganisationLogo) ? chkObjExist.OrganisationLogo : OrganisationLogo;
                        chkObjExist.OrganisationBanner = string.IsNullOrWhiteSpace(OrganisationBanner) ? chkObjExist.OrganisationBanner : OrganisationBanner;
                        chkObjExist.Status = orgObj.Status;
                        chkObjExist.IsUserAssignment = orgObj.IsUserAssignment;
                        chkObjExist.MaxUsers = orgObj.MaxUsers;
                        chkObjExist.ExpiryDate = DateTime.ParseExact(orgObj.ExpDate + " 23:59:59", ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(), CultureInfo.InvariantCulture);

                        chkObjExist.DateLastModified = DateTime.Now;
                        chkObjExist.LastModifiedByID = Convert.ToInt64(Session["UserID"]);
                        db.SaveChanges();
                        var LanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;
                        OrganisationInfo orgInfo = db.OrganisationInfoes.Where(x => x.OrganisationID == chkObjExist.OrganisationID && x.LanguageId == LanguageId).FirstOrDefault();
                        if (orgInfo != null)
                        {
                            orgInfo.OrganisationID = orgObj.OrganisationID;
                            orgInfo.OrganisationUID = orgObj.OrganisationUID;
                            orgInfo.OrganisationName = orgObj.OrganisationName;
                            orgInfo.AddressLine1 = orgObj.AddressLine1;
                            orgInfo.AddressLine2 = orgObj.AddressLine2;
                            orgInfo.AddressLine3 = orgObj.AddressLine3;
                            orgInfo.PostalCode = orgObj.PostalCode;
                            orgInfo.OrganisationLogo = string.IsNullOrWhiteSpace(OrganisationLogo) ? chkObjExist.OrganisationLogo : OrganisationLogo;
                            orgInfo.OrganisationBanner = string.IsNullOrWhiteSpace(OrganisationBanner) ? chkObjExist.OrganisationBanner : OrganisationBanner;
                            orgInfo.Country = orgObj.Country;
                            orgInfo.LanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;
                            orgInfo.LastModifiedByID = Convert.ToInt64(Session["UserID"]);
                            orgInfo.DateLastModified = DateTime.Now;
                            db.SaveChanges();
                        }
                        else
                        {
                            orgInfo = new OrganisationInfo();
                            orgInfo.OrganisationID = orgObj.OrganisationID;
                            orgInfo.OrganisationUID = orgObj.OrganisationUID;
                            orgInfo.OrganisationName = orgObj.OrganisationName;
                            orgInfo.AddressLine1 = orgObj.AddressLine1;
                            orgInfo.AddressLine2 = orgObj.AddressLine2;
                            orgInfo.AddressLine3 = orgObj.AddressLine3;
                            orgInfo.PostalCode = orgObj.PostalCode;
                            orgInfo.OrganisationLogo = string.IsNullOrWhiteSpace(OrganisationLogo) ? chkObjExist.OrganisationLogo : OrganisationLogo;
                            orgInfo.OrganisationBanner = string.IsNullOrWhiteSpace(OrganisationBanner) ? chkObjExist.OrganisationBanner : OrganisationBanner;
                            orgInfo.Country = orgObj.Country;
                            orgInfo.LanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;
                            orgInfo.CreatedById = Convert.ToInt64(Session["UserID"]);
                            orgInfo.CreationDate = DateTime.Now;
                            db.OrganisationInfoes.Add(orgInfo);
                            db.SaveChanges();
                        }



                        // Remove Organisation id from groups
                        //db = new LeopinkLMSDBEntities();
                        var b = from x in db.Groups
                                where x.Status == true && x.OrganisationID == chkObjExist.OrganisationID
                                select x;
                        //foreach (var y in b.ToList())
                        //{
                        //    var z = db.Groups.Find(y.GroupID);
                        //    z.OrganisationID = null;
                        //    try
                        //    {
                        //        db.SaveChanges();
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                        //    }
                        //}

                        #region remove unselected
                        foreach (var y in b.ToList())
                        {
                            Boolean RemoveThis = ia.Contains(y.GroupID);
                            if (!RemoveThis)
                            {
                                y.OrganisationID = null;
                                y.MaxUsers = 0;
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        foreach (var _gid in ia)
                        {
                           

                            var Newgroups = db.Groups.Where(x => x.GroupID == _gid && x.Status == true && x.Organisation == null).FirstOrDefault();
                            if (Newgroups != null)
                            {
                                Newgroups.OrganisationID = chkObjExist.OrganisationID;
                                Newgroups.MaxUsers = chkObjExist.MaxUsers;
                                db.SaveChanges();
                            }
                        }
                        //using (db)
                        //{
                        //    var some = db.Groups.Where(x => x.Status == true && x.OrganisationID == chkObjExist.OrganisationID).ToList();
                        //    some.ForEach(a => a.OrganisationID = null);
                        //    db.SaveChanges();
                        //}

                        // Add organisation id to groups
                        //db = new LeopinkLMSDBEntities();
                        //using (db)
                        //{
                        //    var some = db.Groups.Where(x => ia.Contains(x.GroupID) && x.Status == true && x.OrganisationID == null).ToList();
                        //    some.ForEach(a => a.OrganisationID = chkObjExist.OrganisationID);
                        //    db.SaveChanges();
                        //}
                        return RedirectToAction("Index", "Organisation");
                    }
                    else
                    {
                        ModelState.AddModelError("OrganisationID", LMSResourse.Admin.Organisation.msgReqOrganisationGroup);
                    }

                }
                else
                {
                    if (chkObjExistTemp.Where(x => x.OrganisationUID.ToLower().Trim() == orgObj.OrganisationUID.ToLower().Trim()).Select(x => x).Count() > 0)
                        ModelState.AddModelError("OrganisationUID", LMSResourse.Admin.Organisation.msgDupOrganisationID);
                    if (chkObjExistTemp.Where(x => x.OrganisationName.ToLower().Trim() == orgObj.OrganisationName.ToLower().Trim()).Select(x => x).Count() > 0)
                        ModelState.AddModelError("OrganisationName", LMSResourse.Admin.Organisation.msgDupOrganisationName);
                }
            
            var model = new GroupLocalView();
            var selectedGroupsLocal = chkObjExist.Groups.Select(grp => new GroupLocal { ID = grp.GroupID, GroupName = grp.GroupName, IsSelected = true }).ToList();
            model.AvailableGroups = db.Groups.Where(grp => grp.IsDeleted == false && grp.Status == true && (grp.Organisation == null || grp.OrganisationID == orgObj.OrganisationID)).Select(grp => new GroupLocal { ID = grp.GroupID, GroupName = grp.GroupName, IsSelected = (ia.Contains(grp.GroupID) ? true : false) }).ToList();
            model.SelectedGroups = selectedGroupsLocal;
            orgObj.GroupListView = model;  
            return View(orgObj);
        }

        #endregion

        #region // Delete Organisation
        [HttpPost]
        public string DeleteOrganisation(int id =0)
        {
            var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
            var OrgExist = db.Organisations.Find(id);
            if (OrgExist != null)
            {
                var OrgLink = from x in db.Organisations
                              join y in db.UserProfiles on x.OrganisationID equals y.OrganisationID  
                              where x.OrganisationID == OrgExist.OrganisationID
                              select x;
                if (OrgLink.Count() == 0)
                {
                    var SelectedGroups = from x in db.Groups
                                         where x.OrganisationID == OrgExist.OrganisationID
                                         select x;
                    foreach(var y in SelectedGroups.ToList())
                    {
                        y.OrganisationID = null;
                        db.SaveChanges();
                    }
                    OrgExist.IsDeleted = true;
                    OrgExist.DeleteInformation = " : " + OrgExist.OrganisationName + " is delete by userName : " + db.UserProfiles.Find(currentLoginUser).EmailAddress + " on date" + DateTime.Now.ToString();
                    db.SaveChanges();
                }
                else
                {
                    return string.Format(LMSResourse.Admin.Organisation.msgDeleteOrganisation,OrgExist.OrganisationName);
                }
            }
            else
                return LMSResourse.Admin.Organisation.msgInvalidOrganisation;
            return "";

        }
        
        #endregion
        
    }
    #region // Organization License
    public class LicenceController : Controller
    {
        private LeopinkLMSDBEntities db = new LeopinkLMSDBEntities();

        #region // Organization License listing
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AjaxHandlerLicence(jQueryDataTableParamModel param)
        {
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
            Func<License, string> orderingFunction = (c => sortColumnIndex == 0 ? ((c.Organisation != null) ? c.Organisation.OrganisationName.TrimEnd().TrimStart().ToLower() : "-") :
                                                        sortColumnIndex == 1 ? ((c.MaxUsers != null) ? c.MaxUsers.ToString().TrimEnd().TrimStart().ToLower() : "-") :
                                                        c.Organisation.OrganisationName.ToLower());
            var sortDirection = Request["sSortDir_0"];
            IEnumerable<License> filterLicense = null;

            /// search action
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filterLicense = from lic in db.Licenses
                              where lic.Organisation.OrganisationName.ToLower().Contains(param.sSearch.ToLower()) ||
                              lic.MaxUsers.ToString().Contains(param.sSearch.ToLower())
                              select lic;

            }
            else
            {
                filterLicense = from lic in db.Licenses
                              orderby lic.Organisation.OrganisationName.ToLower() descending
                              select lic;

            }

            // ordering action
            // ordering action
            if (sortColumnIndex == 2)
            {
                filterLicense = (sortDirection == "asc") ? filterLicense.OrderBy(lic => lic.ExpiryDate) : filterLicense.OrderByDescending(lic => lic.ExpiryDate);
            }
            else if (sortColumnIndex == 3)
            {
                filterLicense = (sortDirection == "asc") ? filterLicense.OrderBy(lic => lic.CreationDate) : filterLicense.OrderByDescending(lic => lic.CreationDate);
            }
            else
                if (sortDirection == "asc")
                {
                    filterLicense = filterLicense.OrderBy(orderingFunction);
                }
                else if (sortDirection == "desc")
                {
                    filterLicense = filterLicense.OrderByDescending(orderingFunction);
                }


            // records to display            
            var displayedLicense = filterLicense.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            if (param.iDisplayLength == -1)
                displayedLicense = filterLicense;

            var result = from obj in displayedLicense.ToList()
                         select new[] {                              
                              ((obj.Organisation==null)?"-": obj.Organisation.OrganisationName),
                              (string.IsNullOrWhiteSpace(obj.MaxUsers.ToString())?"-":obj.MaxUsers.ToString()),
                              string.Format("{0:dd/MM/yyyy}",obj.ExpiryDate),
                              string.Format("{0:dd/MM/yyyy HH:mm}",obj.CreationDate),
                              Convert.ToString(obj.LicenseID)
                          };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = filterLicense.Count(),
                iTotalDisplayRecords = filterLicense.Count(),
                aaData = result
            },
            JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region // Create Organisation Licence
        public ActionResult CreateLicence()
        {
            ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
            var model = new OrganisationLicence();
            model.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.
            
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateLicence(OrganisationLicence Objlic)
        {
            ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
            Objlic.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.
            if (ModelState.IsValid) // check the model is validate or not.
            {
                var checkOrganisation = db.Licenses.Where(lic => lic.OrganisationID == Objlic.OrganisationID).Select(cat => cat).SingleOrDefault();
                if (checkOrganisation == null) // find the Organisation licence already exist.
                {
                    var usercount = db.UserProfiles.Where(usr => usr.OrganisationID == Objlic.OrganisationID).Count();
                    if (Objlic.MaxUsers > usercount)
                    {
                        // save the license object in database.
                        License ObjLicense = new License(); // create object of License table to save the record
                        ObjLicense.OrganisationID = Objlic.OrganisationID;
                        ObjLicense.MaxUsers = Objlic.MaxUsers;
                        ObjLicense.ExpiryDate = DateTime.ParseExact(Objlic.ExpDate + " 23:59:59", ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(), CultureInfo.InvariantCulture);
                        ObjLicense.CreatedById = Convert.ToInt64(Session["UserID"]);
                        ObjLicense.CreationDate = DateTime.Now;
                        db.Licenses.Add(ObjLicense);
                        db.SaveChanges(); // save data in license table in database

                        return RedirectToAction("Index", "Licence"); // redirect to category index page.
                    }
                    else
                    {
                        ModelState.AddModelError("OrganisationID", LMSResourse.Admin.Organisation.msgLicNoOfUserAlert);
                    }
                }
                else
                {
                    ModelState.AddModelError("OrganisationID", LMSResourse.Admin.Organisation.msgDupOrgLicence); // if org licence already exists then return the model with error messages.
                }
            }
            return View(Objlic);
        }
        #endregion

        #region // Edit Organisation Licence
        public ActionResult EditLicence(int id = 0)
        {
            License lic = db.Licenses.Find(id);
            OrganisationLicence ObjOrglic = new OrganisationLicence();
            ObjOrglic.LicenseID = lic.LicenseID;
            if (lic == null) { return HttpNotFound(); }
            else
            {
                if (lic.OrganisationID != null)
                {
                    ObjOrglic.OrganisationID = lic.OrganisationID;
                    ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName", lic.OrganisationID);
                }
                else
                {
                    ObjOrglic.OrganisationID = 0;
                    ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
                }
                ObjOrglic.MaxUsers = lic.MaxUsers;
                ObjOrglic.ExpiryDate = lic.ExpiryDate;
                ObjOrglic.ExpDate = lic.ExpiryDate.ToString(ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString());
                ObjOrglic.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.
            }
            return View(ObjOrglic);
        }

        [HttpPost]
        public ActionResult EditLicence(OrganisationLicence Objlic)
        {
            ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
            Objlic.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.

            if (ModelState.IsValid)
            {
                var duplicateOrgLic = db.Licenses.Where(lic => lic.OrganisationID == Objlic.OrganisationID && lic.LicenseID != Objlic.LicenseID).FirstOrDefault();
                if (duplicateOrgLic == null)
                {
                    var usercount = db.UserProfiles.Where(usr => usr.OrganisationID == Objlic.OrganisationID).Count();
                    if (Objlic.MaxUsers >= usercount)
                    {
                        var ObjLicense = db.Licenses.Find(Objlic.LicenseID);
                        if (ObjLicense != null)
                        {
                            ObjLicense.OrganisationID = Objlic.OrganisationID;
                            ObjLicense.MaxUsers = Objlic.MaxUsers;
                            ObjLicense.ExpiryDate = DateTime.ParseExact(Objlic.ExpDate, ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(), CultureInfo.InvariantCulture);
                            ObjLicense.LastModifiedByID = Convert.ToInt64(Session["UserID"]);
                            ObjLicense.LastModifiedDate = DateTime.Now;
                            db.SaveChanges();

                            return RedirectToAction("Index", "Licence");
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("OrganisationID", LMSResourse.Admin.Organisation.msgLicNoOfUserAlert);
                    }
                }
                else
                {
                    // return model with error message.
                    ModelState.AddModelError("OrganisationID", LMSResourse.Admin.Organisation.msgDupOrgLicence);
                }
            }
            return View(Objlic);
        }
        #endregion
    }
    #endregion
}