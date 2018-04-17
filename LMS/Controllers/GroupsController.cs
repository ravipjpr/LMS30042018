using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CLSLms;
using LMS.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Configuration;
using System.Globalization;

namespace LMS.Controllers
{
    [CustomAuthorize]
    public class GroupsController : Controller
    {
        private LeopinkLMSDBEntities db = new LeopinkLMSDBEntities();

        #region /// Group listing
        
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult AjaxHandlerGroups(jQueryDataTableParamModel param)
        {
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
            Func<Group, string> orderingFunction = (c => sortColumnIndex == 0 ? c.GroupName.TrimEnd().TrimStart().ToLower() :
                                                        sortColumnIndex == 1 ? ((c.Organisation != null) ? c.Organisation.OrganisationName.TrimEnd().TrimStart().ToLower() : "-") :
                                                        sortColumnIndex == 2 ? ((c.GroupManager != null) ? c.GroupManager.TrimEnd().TrimStart().ToLower() : "-") :
                                                        sortColumnIndex == 3 ? ((c.EmailAddress != null) ? c.EmailAddress.TrimEnd().TrimStart().ToLower() : "-") :
                                                        sortColumnIndex == 4 ? ((c.ContactNo != null) ? c.ContactNo.TrimEnd().TrimStart().ToLower() : "-") :
                                                        sortColumnIndex == 5 ? (c.Status.ToString()) :
                                                        c.GroupName.ToLower());
            var sortDirection = Request["sSortDir_0"];
            IEnumerable<Group> filterGroup = null;

            /// search action
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filterGroup = from grp in db.Groups
                              where grp.GroupName.ToLower().Contains(param.sSearch.ToLower()) ||
                              grp.Organisation.OrganisationName.ToLower().Contains(param.sSearch.ToLower()) ||
                              grp.GroupManager.ToLower().Contains(param.sSearch.ToLower())
                              select grp;

            }
            else
            {
                filterGroup = from grp in db.Groups
                              orderby grp.GroupName.ToLower() descending, grp.Organisation.OrganisationName.ToLower() descending, grp.GroupManager.ToLower() descending
                              select grp;

            }

            // ordering action
            if (sortColumnIndex == 7)
            {
                filterGroup = (sortDirection == "asc") ? filterGroup.OrderBy(grp => grp.DateLastModified) : filterGroup.OrderByDescending(grp => grp.DateLastModified);
            }
            else
                if (sortDirection == "asc")
                {
                    filterGroup = filterGroup.OrderBy(orderingFunction);
                }
                else if (sortDirection == "desc")
                {
                    filterGroup = filterGroup.OrderByDescending(orderingFunction);
                }

            if ((Session["UserRoles"].ToString()).Split(',').Select(int.Parse).ToArray().Contains(2))
            {
                var GroupAdminGroups = db.UserGroups.Where(x => x.UserId == currentLoginUser).Select(x => x.GroupID).ToList();
                filterGroup = from x in filterGroup.ToList()
                              where GroupAdminGroups.Contains(x.GroupID)
                              select x;
            }
            else
            {
                filterGroup = filterGroup.ToList();
            }
            filterGroup = filterGroup.Where(x=>x.IsDeleted == false).ToList();
            

            // records to display            
            var displayedGroup = filterGroup.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            if (param.iDisplayLength == -1)
                displayedGroup = filterGroup;
            var ActiveStatus = LMSResourse.Common.Common.lblActiveStatus;
            var InactiveStatus = LMSResourse.Common.Common.lblInactiveStatus;
            var result = from obj in displayedGroup.ToList()
                         select new[] {                              
                              obj.GroupName,
                              ((obj.Organisation==null)?"-": obj.Organisation.OrganisationName),
                              (string.IsNullOrWhiteSpace(obj.MaxUsers.ToString())?"0":obj.MaxUsers.ToString()),                              
                              (!obj.AssignedUsers.HasValue?"0":obj.AssignedUsers.Value.ToString()),
                              (string.IsNullOrWhiteSpace(obj.ContactNo)?"-":obj.ContactNo),
                              ((obj.Status)?ActiveStatus :InactiveStatus ), 
                              Convert.ToString(obj.GroupID),
                              ((obj.Organisation==null)?"false": Convert.ToString(obj.Organisation.IsUserAssignment)),

                          };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = filterGroup.Count(),
                iTotalDisplayRecords = filterGroup.Count(),
                aaData = result
            },
                           JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region /// Create Group
        
        public ActionResult CreateGroup()
        {
            ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.ExpiryDate >= DateTime.Now && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
            var model = new _Group();
            model.Status = true;
            model.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.

             //ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString(); // sets the client side date format.
            return View(model);
        }

        /// <summary>
        /// http post method of create group 
        /// </summary>
        /// <param name="gr"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateGroup(_Group gr)
        {
            ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.ExpiryDate >= DateTime.Now && org.IsDeleted == false).Select(org => org), "OrganisationID", "OrganisationName");
            gr.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString();
               
            if (ModelState.IsValid) // model validation
            {
                var checkGroupName = db.Groups.Where(grp => grp.GroupName.TrimEnd().TrimStart().ToLower() == gr.GroupName.TrimEnd().TrimStart().ToLower()).Select(grp => grp).SingleOrDefault();
                if (checkGroupName == null) // check the duplicate name for group 
                {
                    var maxuserAllowed = db.Organisations.Where(a => a.OrganisationID == gr.OrganisationID).FirstOrDefault();
                    if (maxuserAllowed != null && maxuserAllowed.MaxUsers < gr.MaxUsers)
                    {

                        ModelState.AddModelError("maxnoofuser", LMSResourse.Admin.Group.msgErrorGreaterthan + maxuserAllowed.MaxUsers);
                        return View(gr);
                    }
                   
                    var temp = ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString();
                    Group grp = new Group();
                    grp.Status = gr.Status;           
                    grp.GroupName = gr.GroupName;
                    grp.GroupDescription = gr.GroupDescription;
                    grp.GroupManager = gr.GroupManager;
                    grp.EmailAddress = gr.EmailAddress;
                    grp.ContactNo = gr.ContactNo;
                    if (gr.OrganisationID != null && gr.OrganisationID.ToString() != "")
                    {
                        grp.OrganisationID = gr.OrganisationID;
                        grp.MaxUsers = gr.MaxUsers;                      
                    }
                    grp.CreationDate = DateTime.Now;
                    grp.CreatedById = Convert.ToInt64(Session["UserID"]);
                    grp.IsDeleted = false;
                    db.Groups.Add(grp);
                    db.SaveChanges(); // save th group data in database

                    GroupInfo grInfo = new GroupInfo(); // create group info record 
                    grInfo.GroupID = grp.GroupID;
                    grInfo.LanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;
                    grInfo.GroupName = gr.GroupName;
                    grInfo.GroupDescription = gr.GroupDescription;
                    grInfo.GroupManager = gr.GroupManager;
                    grInfo.EmailAddress = gr.EmailAddress;
                    grInfo.ContactNo = gr.ContactNo;
                    grInfo.CreatedById = grp.CreatedById;
                    grInfo.CreationDate = grp.CreationDate;
                    db.GroupInfoes.Add(grInfo);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Groups");
                }
                else // return model with error message
                {
                    ModelState.AddModelError("GroupName", LMSResourse.Admin.Group.errDupGroup);
                }
            }
            var li = ModelState.Values.ToArray();
            ModelState item = li[7];
            if (item != null && item.Errors.Count > 0)
            {
                ModelState.AddModelError("maxnoofuser", LMSResourse.Admin.Group.msgErrorIntegerOnly);
            }
           
            return View(gr);
        }

        #endregion


        #region /// Create Group partial view

        public ActionResult _CreateGroup()
        {
            ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
            var model = new _Group();
            model.Status = true;
            return PartialView(model);
        }

        /// <summary>
        /// http post method of create group 
        /// </summary>
        /// <param name="gr"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _CreateGroup(_Group gr)
        {
            if (ModelState.IsValid) // model validation
            {
                var checkGroupName = db.Groups.Where(grp => grp.GroupName.TrimEnd().TrimStart().ToLower() == gr.GroupName.TrimEnd().TrimStart().ToLower()).Select(grp => grp).SingleOrDefault();
                if (checkGroupName == null) // check the duplicate name for group 
                {
                    Group grp = new Group();
                    grp.Status = true;
                    grp.GroupName = gr.GroupName;
                    grp.GroupDescription = gr.GroupDescription;
                    grp.GroupManager = gr.GroupManager;
                    grp.EmailAddress = gr.EmailAddress;
                    grp.ContactNo = gr.ContactNo;  
                
                    grp.CreationDate = DateTime.Now;
                    grp.CreatedById = Convert.ToInt64(Session["UserID"]);
                    grp.IsDeleted = false;
                    db.Groups.Add(grp);
                    db.SaveChanges(); // save th group data in database

                    GroupInfo grInfo = new GroupInfo(); // create group info record 
                    grInfo.GroupID = grp.GroupID;
                    grInfo.LanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;
                    grInfo.GroupName = grp.GroupName;
                    grInfo.GroupDescription = gr.GroupDescription;
                    grInfo.GroupManager = grp.GroupManager;
                    grInfo.EmailAddress = grp.EmailAddress;
                    grInfo.ContactNo = grp.ContactNo;
                    grInfo.CreatedById = grp.CreatedById;
                    grInfo.CreationDate = grp.CreationDate;
                    db.GroupInfoes.Add(grInfo);
                    db.SaveChanges();
                    return Json(new { success = true, msg = grp.GroupName, gid = grp.GroupID });
                }
                else // return model with error message
                {
                    ModelState.AddModelError("GroupName", LMSResourse.Admin.Group.errDupGroup);
                    return Json(new { success=false,msg="Astrix (*) fields are required. "});
                }
            }
            ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).Select(org => org), "OrganisationID", "OrganisationName");
            return Json(new { success = false, msg = JsonConvert.SerializeObject(ModelState.Where(a=>a.Value.Errors.Count()>0).ToList())});
            //return PartialView(gr);
        }

        #endregion

        #region // Edit Group
        /// <summary>
        /// http get method of edit group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditGroup(int id = 0)
        {
            _Group gr = new _Group();

            Group grp = db.Groups.Find(id);
            if (grp == null)  { return HttpNotFound(); }
            else
            {
                gr.GroupID = grp.GroupID;
                gr.Status = grp.Status;
                gr.GroupName = grp.GroupName;
                gr.GroupDescription = grp.GroupDescription;
                gr.GroupManager = grp.GroupManager;
                gr.EmailAddress = grp.EmailAddress;
                gr.ContactNo = grp.ContactNo;
                gr.OrganisationID = grp.OrganisationID;
                gr.MaxUsers = grp.MaxUsers;
               


                var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
                if ((Session["UserRoles"].ToString()).Split(',').Select(int.Parse).ToArray().Contains(2))
                {
                    var GroupAdminGroups = db.UserGroups.Where(x => x.UserId == currentLoginUser).Select(x => x.GroupID).ToList();
                    if(!GroupAdminGroups.Contains(grp.GroupID))
                    {
                        return RedirectToAction("Index", "Groups");
                    }
                }
                if (grp.OrganisationID != null)
                {
                    ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName", grp.OrganisationID);
                }
                else
                {
                    ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true && org.IsDeleted == false).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
                }
                if (grp.OrganisationID != null)
                {
                    // check is this group is the last group which is assigned to organisation if yes then it can't be updated as this is the last group which is assigned to this organisation.
                    var OrgCount = db.Groups.Where(x => x.OrganisationID == grp.OrganisationID && grp.IsDeleted == false).Count();
                    if (OrgCount == 1)
                        ViewBag.IsOrgBlock = "1";
                    else
                        ViewBag.IsOrgBlock = "0";                    
                    // check is this group have any user which is active or inactive then it can't be updated else it can me moved to any organisation.
                    var UserCount = db.UserGroups.Where(x => x.UserProfile.IsDelete == false).Count();
                    if(UserCount>0)
                        ViewBag.IsOrgBlock = "1";
                }
                else
                    ViewBag.IsOrgBlock = "0";
            }
            return View(gr);
        }

        /// <summary>
        /// http post method of edit group
        /// </summary>
        /// <param name="Objgrp"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditGroup(_Group Objgrp)
        {
            var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
            if ((Session["UserRoles"].ToString()).Split(',').Select(int.Parse).ToArray().Contains(2)) // group administrator can not edit the group.
            {
                var GroupAdminGroups = db.UserGroups.Where(x => x.UserId == currentLoginUser).Select(x => x.GroupID).ToList();
                if (!GroupAdminGroups.Contains(Objgrp.GroupID))
                {
                    return RedirectToAction("Index", "Groups");
                }
            }
            if (Objgrp.OrganisationID != null) // if organisation is not selected 
            {
                ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName", Objgrp.OrganisationID);
            }
            else // if organisation selected
            {
                ViewBag.OrgList = new SelectList(db.Organisations.Where(org => org.Status == true).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName");
            }

           
            if (ModelState.IsValid) // model validation
            {
                var duplicateTile = db.Groups.Where(grp => grp.GroupName == Objgrp.GroupName && grp.GroupID != Objgrp.GroupID).FirstOrDefault();
                if (duplicateTile == null) // check duplicate group name 
                {
                    var dbObjgrp = db.Groups.Find(Objgrp.GroupID); // find the group
                    if (dbObjgrp != null) // if not the return the model with error message.
                    {
                        if(dbObjgrp.OrganisationID != null)
                        {
                            // check is this group is the last group which is assigned to organisation if yes then it can't be updated as this is the last group which is assigned to this organisation.
                            var OrgCount = db.Groups.Where(x=>x.OrganisationID == dbObjgrp.OrganisationID && x.IsDeleted == false).Count();
                            if (OrgCount <= 1 && Objgrp.OrganisationID == null)
                            {
                                ModelState.AddModelError("OrganisationID", "Organisation can be null. as this is the last group assigned to the organisation.");
                                return View(Objgrp);                                
                            }
                        }
                        var isOrgNull = Objgrp.OrganisationID != null ? Objgrp.OrganisationID : 0;
                        var maxuserAllowed = db.Organisations.Where(a => a.OrganisationID == isOrgNull).FirstOrDefault();
                        if (maxuserAllowed != null && maxuserAllowed.MaxUsers >= Objgrp.MaxUsers)
                        { 
                            
                        }
                        else
                        {
                            if (isOrgNull != 0)
                            {
                                ModelState.AddModelError("maxnoofuser", LMSResourse.Admin.Group.msgErrorGreaterthan + (Objgrp.MaxUsers == 0 ? "" : maxuserAllowed == null ? " allowed" : maxuserAllowed.MaxUsers.ToString()));
                                return View(Objgrp);
                            }
                        }

                        var membersIngroup = db.Groups.Where(a => a.GroupID == Objgrp.GroupID).FirstOrDefault().UserGroups.Count();
                        if (dbObjgrp.OrganisationID != null)
                        {
                        if ((maxuserAllowed != null && maxuserAllowed.MaxUsers >= Objgrp.MaxUsers) && membersIngroup <= Objgrp.MaxUsers) { }
                        else
                        {
                            ModelState.AddModelError("maxnoofuser", "No of users can not change. Because there is already " + membersIngroup + " members in this group");
                            return View(Objgrp);
                        }
                    }
                        // update the existing record.
                        dbObjgrp.GroupID = Objgrp.GroupID;
                        dbObjgrp.GroupName = Objgrp.GroupName;
                        dbObjgrp.GroupDescription = Objgrp.GroupDescription;
                        dbObjgrp.OrganisationID = Objgrp.OrganisationID != null ? Objgrp.OrganisationID : null;
                        //dbObjgrp.Organisation = (Objgrp.Organisation != null) ? Objgrp.Organisation : null;
                        dbObjgrp.GroupManager = Objgrp.GroupManager;
                        dbObjgrp.EmailAddress = Objgrp.EmailAddress;
                        dbObjgrp.ContactNo = Objgrp.ContactNo;
                        dbObjgrp.DateLastModified = DateTime.Now;
                        dbObjgrp.LastModifiedByID = Convert.ToInt64(Session["UserID"]);
                        dbObjgrp.Status = Objgrp.Status;
                        if (Objgrp.OrganisationID != null && Objgrp.OrganisationID.ToString() != "")
                        {
                            dbObjgrp.MaxUsers = Objgrp.MaxUsers;
                        }
                        else {
                            dbObjgrp.MaxUsers = 0;
                        }
                        db.SaveChanges(); // save the data of group

                        var LanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;
                        var gInfo = db.GroupInfoes.Where(x => x.GroupID == dbObjgrp.GroupID && x.LanguageId == LanguageId).FirstOrDefault();
                        if(gInfo!=null) // if group info information in not exist then create a new record else update the existing record.
                        {
                            // update group info record.
                            gInfo.GroupID = dbObjgrp.GroupID;
                            gInfo.LanguageId = LanguageId;
                            gInfo.GroupName = dbObjgrp.GroupName;
                            gInfo.GroupDescription = dbObjgrp.GroupDescription;
                            gInfo.GroupManager = dbObjgrp.GroupManager;
                            gInfo.EmailAddress = dbObjgrp.EmailAddress;
                            gInfo.ContactNo = dbObjgrp.ContactNo;
                            gInfo.LastModifiedByID = dbObjgrp.LastModifiedByID;
                            gInfo.DateLastModified = dbObjgrp.DateLastModified;                           
                            db.SaveChanges();
                        }
                        else // create new record for groupinfo table
                        {
                            gInfo = new GroupInfo();
                            gInfo.GroupID = dbObjgrp.GroupID;
                            gInfo.LanguageId = LanguageId;
                            gInfo.GroupName = dbObjgrp.GroupName;
                            gInfo.GroupDescription = dbObjgrp.GroupDescription;
                            gInfo.GroupManager = dbObjgrp.GroupManager;
                            gInfo.EmailAddress = dbObjgrp.EmailAddress;
                            gInfo.ContactNo = dbObjgrp.ContactNo;
                            gInfo.CreatedById = Convert.ToInt64(Session["UserID"]);
                            gInfo.CreationDate = DateTime.Now;
                            db.GroupInfoes.Add(gInfo);
                            db.SaveChanges();
                        }
                        return RedirectToAction("Index", "Groups");
                    }
                    else
                    {
                        return HttpNotFound(); // if record not fond.
                    }
                }
                else // return model with error message.
                {
                    ModelState.AddModelError("GroupName", LMSResourse.Admin.Group.errDupGroup);
                }
            }
            // if organistion is not selected then return the organisation list.
            ViewBag.OrgList = (Objgrp.OrganisationID == null) ? new SelectList(db.Organisations.Where(org => org.Status == true).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName") : new SelectList(db.Organisations.Where(org => org.Status == true).OrderBy(org => org.OrganisationName).Select(org => org), "OrganisationID", "OrganisationName", Objgrp.OrganisationID);
            var li = ModelState.Values.ToArray();
           ModelState item= li[8];
           if(item!=null && item.Errors.Count>0)
           {
               ModelState.AddModelError("maxnoofuser", LMSResourse.Admin.Group.msgErrorIntegerOnly);
           }
            return View(Objgrp);

        }
        #endregion

        #region // Create Group HomePage


        public ActionResult GroupHomePage(int id = 0)
        {
            GroupHomepageLocal Obj = new GroupHomepageLocal();
            bool preview = false;
            if (Request.QueryString != null)
            {
                if (Request.QueryString["Preview"] != null)
                    preview = true;
            }

            var group = db.Groups.Find(id);
            if (group != null)
            {
                var gHomepage = db.GetHomePage(group.GroupID, 0);
                var y = from x in gHomepage
                        select new GroupHomepageLocal { GroupId = x.GroupID, GroupHomePageId = x.GroupHomepageID == null ? 0 : x.GroupHomepageID, GroupName = x.GroupName, OrganisationName = x.OrganisationName, ImagePosition = x.ImageLocation == null ? (int)1 : (int)x.ImageLocation, PageContent = WebUtility.HtmlDecode(x.HomePageContent), UploadFileUrl = x.ImagePath, IsPreview = preview };
                Obj = y.SingleOrDefault();
                //if (Obj != null) { Obj.GroupName = group.GroupName; Obj.ImagePosition = string.IsNullOrWhiteSpace(Obj.ImagePosition.ToString()) ? 1 : Obj.ImagePosition; }
                //else { Obj = new GroupHomepageLocal(); Obj.GroupId = group.GroupID; Obj.OrganisationName = (group.Organisation != null) ? group.Organisation.OrganisationName : ""; Obj.GroupName = group.GroupName; Obj.ImagePosition = 1; }

                if (Obj == null){ Obj = new GroupHomepageLocal(); Obj.GroupId = group.GroupID; Obj.OrganisationName = (group.Organisation != null) ? group.Organisation.OrganisationName : ""; Obj.GroupName = group.GroupName; Obj.ImagePosition = 1; }
            }
            else { }
            return View(Obj);
        }
        /// <summary>
        /// http post method to save the group home page of a group.
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GroupHomePage(GroupHomepageLocal Obj)
        {
            if (ModelState.IsValid)
            {

                string fileName = "";
                var GHomePage = db.GroupHomepages.Where(x => x.GroupID == Obj.GroupId).FirstOrDefault();
                if (GHomePage == null) // create new
                {
                    GroupHomepage Objnew = new GroupHomepage();
                    #region // Process Course file Save to server folders
                    if (Request.Files.Count > 0)
                    {
                        foreach (string file in Request.Files)
                        {
                            HttpPostedFileBase hpf = Request.Files[file];

                            if (hpf.FileName != "")
                            {
                                fileName = hpf.FileName.Split('\\').Last().ToString();

                                if (Common.IsValidFileName(hpf.FileName.Split('\\').Last().ToString(), true) == false)
                                {
                                    ModelState.AddModelError("UploadFileUrl", "error");
                                    //PrepareAvailableLangaugeModel(activity, false);
                                    return View(Obj);
                                }

                                Guid g = Guid.NewGuid();

                                Objnew.GroupID = Obj.GroupId;
                                Objnew.ImagePath = "";
                                Objnew.ImageLocation = Obj.ImagePosition;
                                Objnew.CreationDate = DateTime.Now;
                                Objnew.CreatedById = Convert.ToInt64(Session["UserID"]);
                                db.GroupHomepages.Add(Objnew);
                                db.SaveChanges();

                                var absImgDir = Path.Combine(Server.MapPath("~") + @"\Content\Uploads\GroupHomepage\", "_" + Objnew.GroupHomepageID.ToString());
                                Directory.CreateDirectory(absImgDir);
                                var relCourseDir = Path.Combine(@"\Content\Uploads\GroupHomepage\", "_" + Objnew.GroupHomepageID.ToString());
                                hpf.SaveAs(Path.Combine(absImgDir, @fileName));
                                Objnew.ImagePath = Path.Combine(relCourseDir, @fileName);

                                db.SaveChanges();
                                Obj.UploadFileUrl = Objnew.ImagePath;
                                AddEditGroupHomepageInfoRecord(Objnew, Obj.PageContent, 1);
                            }
                        }
                    }
                    #endregion

                }
                else
                {
                    if (Request.Files.Count > 0)
                    {
                        foreach (string file in Request.Files)
                        {
                            HttpPostedFileBase hpf = Request.Files[file];

                            if (hpf.FileName != "")
                            {
                                fileName = hpf.FileName.Split('\\').Last().ToString();

                                if (Common.IsValidFileName(hpf.FileName.Split('\\').Last().ToString(), true) == false)
                                {
                                    ModelState.AddModelError("UploadFileUrl", "error");
                                    //PrepareAvailableLangaugeModel(activity, false);
                                    return View(Obj);
                                }
                                var absImgDir = Path.Combine(Server.MapPath("~") + @"\Content\Uploads\GroupHomepage\", "_" + GHomePage.GroupHomepageID.ToString());
                                if (!Directory.Exists(absImgDir))
                                    Directory.CreateDirectory(absImgDir);
                                var relCourseDir = Path.Combine(@"\Content\Uploads\GroupHomepage\", "_" + GHomePage.GroupHomepageID.ToString());
                                hpf.SaveAs(Path.Combine(absImgDir, @fileName));
                                GHomePage.ImagePath = Path.Combine(relCourseDir, @fileName);

                            }
                        }
                    }
                    GHomePage.ImageLocation = Obj.ImagePosition;
                    GHomePage.CreationDate = DateTime.Now;
                    GHomePage.CreatedById = Convert.ToInt64(Session["UserID"]);
                    db.SaveChanges();
                    Obj.UploadFileUrl = GHomePage.ImagePath;
                    AddEditGroupHomepageInfoRecord(GHomePage, Obj.PageContent, 1);
                }
            }
            else
            {
                return View(Obj);
            }
            if (Obj.IsPreview == true)
                return RedirectToAction("GroupHomePage", "Groups", new { id = Obj.GroupId, @Preview = "y" });
            else
                return RedirectToAction("GroupHomePage", "Groups", new { id = Obj.GroupId });

            //return View(Obj);
        }

        public void AddEditGroupHomepageInfoRecord(GroupHomepage ObjG, string Content, int languageId)
        {
            var RecExist = db.GroupHomepageInfoes.Where(x => x.GroupHomepageID == ObjG.GroupHomepageID && x.LanguageId == languageId).FirstOrDefault();
            if (RecExist != null)
            {
                RecExist.HomePageContent = Content;
                RecExist.GroupHomepageID = ObjG.GroupHomepageID;
                RecExist.DateLastModified = DateTime.Now;
                RecExist.LastModifiedById = Convert.ToInt64(Session["UserID"]);
                db.SaveChanges();
            }
            else
            {
                GroupHomepageInfo Objnew = new GroupHomepageInfo();
                Objnew.HomePageContent = Content;
                Objnew.GroupHomepageID = ObjG.GroupHomepageID;
                Objnew.LanguageId = languageId;
                Objnew.CreationDate = DateTime.Now;
                Objnew.CreatedById = Convert.ToInt64(Session["UserID"]);
                db.GroupHomepageInfoes.Add(Objnew);
                db.SaveChanges();
            }
        }

        [AllowAnonymous]
        public ActionResult HomePagePreview(int id = 0)
        {
            if (id == 0) { return RedirectToAction("Unauthorised", "Error"); }
            GroupHomepageLocal Obj = new GroupHomepageLocal();
            bool preview = false;
            if (Request.QueryString != null)
            {
                if (Request.QueryString["Preview"] != null)
                    preview = true;
            }

            var group = db.Groups.Find(id);
            if (group != null)
            {
                var gHomepage = db.GetHomePage(group.GroupID, 0);
                var y = from x in gHomepage
                        select new GroupHomepageLocal { GroupId = x.GroupID, GroupHomePageId = x.GroupHomepageID == null ? 0 : x.GroupHomepageID, GroupName = x.GroupName, OrganisationName = x.OrganisationName, ImagePosition = x.ImageLocation == null ? (int)1 : (int)x.ImageLocation, PageContent = WebUtility.HtmlDecode(x.HomePageContent), UploadFileUrl = x.ImagePath, IsPreview = preview };
                Obj = y.SingleOrDefault();
                //if (Obj != null) { Obj.GroupName = group.GroupName; Obj.ImagePosition = string.IsNullOrWhiteSpace(Obj.ImagePosition.ToString()) ? 1 : Obj.ImagePosition; }
                //else { Obj = new GroupHomepageLocal(); Obj.GroupId = group.GroupID; Obj.OrganisationName = (group.Organisation != null) ? group.Organisation.OrganisationName : ""; Obj.GroupName = group.GroupName; Obj.ImagePosition = 1; }

                if (Obj == null) { Obj = new GroupHomepageLocal(); Obj.GroupId = group.GroupID; Obj.OrganisationName = (group.Organisation != null) ? group.Organisation.OrganisationName : ""; Obj.GroupName = group.GroupName; Obj.ImagePosition = 1; }
            }
            else { return RedirectToAction("Unauthorised", "Error"); }
            return View(Obj);
           
        }


        #endregion

        #region // Assign group course
        public ActionResult AssignedCourse(int id = 0)
        {
            var Groupexist = db.Groups.Find(id);
            GroupAssignedCourse us = new GroupAssignedCourse();

            if (Groupexist != null)
            {
                var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
                if ((Session["UserRoles"].ToString()).Split(',').Select(int.Parse).ToArray().Contains(2))
                {
                    var GroupAdminGroups = db.UserGroups.Where(x => x.UserId == currentLoginUser).Select(x => x.GroupID).ToList();
                    if (!GroupAdminGroups.Contains(Groupexist.GroupID))
                    {
                        return RedirectToAction("Index", "Groups");
                    }
                }

                us.GroupId = Groupexist.GroupID;
                us.GroupName = Groupexist.GroupName;
                us.DateFormatForClientSide = ConfigurationManager.AppSettings["dateformatForCalanderClientSide"].ToString();

            }
            return View(us);
        }

        public ActionResult AjaxHandlerGroupAssignedCourses(jQueryDataTableParamModel param)
        {

            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<AssignedGroupCourse, string> orderingFunction = (c => sortColumnIndex == 1 ? c.CourseName.TrimEnd().TrimStart().ToLower() :
                                                            sortColumnIndex == 2 ? c.CategoryName.TrimEnd().TrimStart().ToLower() :
                                                            sortColumnIndex == 3 ? c.CertificateName.TrimEnd().TrimStart().ToLower() :
                                                            c.CourseName.ToLower());
            var sortDirection = Request["sSortDir_0"];
            IEnumerable<AssignedGroupCourse> filterUserAssignedCourse = null;
            var GroupId = Int64.Parse(param.iD.ToString());
            int languageId = 0;
            languageId = int.Parse(Session["LanguageId"].ToString());
            try
            {
                // Get data from stored procedure 
                var Userid = Convert.ToInt64(Session["UserID"]);

                var _PData = db.GetAssignedGroupCourse(GroupId, Userid, languageId);
                var tempx = from x in _PData
                            select new AssignedGroupCourse
                            {
                                CourseId = x.CourseId,
                                CourseName = x.CourseName,
                                CategoryName = x.CategoryName,
                                CertificateName = x.CertificateName,
                                AssignedStatus = x.AssignedStatus,
                                ExpiryDate = x.ExpiryDate
                            };
                filterUserAssignedCourse = tempx.ToList<AssignedGroupCourse>();

                /// search action
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filterUserAssignedCourse = from x in filterUserAssignedCourse
                                               where x.CourseName.ToLower().Contains(param.sSearch.ToLower()) || x.CertificateName.ToLower().Contains(param.sSearch.ToLower()) || x.CategoryName.ToLower().Contains(param.sSearch.ToLower())
                                               select x;
                }
                else
                {
                    filterUserAssignedCourse = from x in filterUserAssignedCourse
                                               select x;
                }
                // ordering action
                if (sortDirection == "asc")
                {
                    filterUserAssignedCourse = filterUserAssignedCourse.OrderBy(orderingFunction);
                }
                else if (sortDirection == "desc")
                {
                    filterUserAssignedCourse = filterUserAssignedCourse.OrderByDescending(orderingFunction);
                }

                filterUserAssignedCourse = filterUserAssignedCourse.ToList();

                // records to display            
                var displayedUserAssignedCourse = filterUserAssignedCourse.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                if (param.iDisplayLength == -1)
                    displayedUserAssignedCourse = filterUserAssignedCourse;

                var result = from obj in displayedUserAssignedCourse.ToList()
                             select new[] {                              
                              obj.CourseName,
                              obj.CategoryName,
                              obj.CertificateName,
                              obj.ExpiryDate==null?"":((DateTime)obj.ExpiryDate).ToString(ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(),CultureInfo.InvariantCulture),
                              obj.AssignedStatus.ToString(),
                              Convert.ToString(obj.CourseId)
                          };
                // return json data.
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filterUserAssignedCourse.Count(),
                    iTotalDisplayRecords = filterUserAssignedCourse.Count(),
                    aaData = result
                },
                               JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {

                },
                                  JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// http post method to update data of course assignment to groups
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        public void AssignCourseToGroup(List<SubmitGroupAssignment> model)
        {
            foreach (var x in model)
            {
                // check the recod is exist or not
                var RecodrExist = db.GroupCourses.Where(y => y.GroupID == x.GroupId && y.CourseId == x.CourseId).FirstOrDefault();
                // if record exist then update the record
                if (RecodrExist != null) 
                {
                    if (x.AssignmentStatus == true && !string.IsNullOrWhiteSpace(x.AssignmentDate))
                    {
                        RecodrExist.ExpiryDate = DateTime.ParseExact(x.AssignmentDate + " 23:59:59", ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(), CultureInfo.InvariantCulture);
                        RecodrExist.AssignedStatus = true;
                    }
                    else
                    {
                        RecodrExist.ExpiryDate = null;
                        RecodrExist.AssignedStatus = false;
                    }
                    RecodrExist.DateLastModified = DateTime.Now;
                    RecodrExist.LastModifiedByID = Convert.ToInt64(Session["UserID"]);
                    db.SaveChanges();
                } // create the new record of group and course relationship.
                else if (x.AssignmentStatus == true && !string.IsNullOrWhiteSpace(x.AssignmentDate))
                {
                    GroupCourse NewRecord = new GroupCourse();
                    NewRecord.CourseId = x.CourseId;
                    NewRecord.GroupID = x.GroupId;
                    NewRecord.ExpiryDate = DateTime.ParseExact(x.AssignmentDate + " 23:59:59", ConfigurationManager.AppSettings["dateformatForCalanderServerSide"].ToString(), CultureInfo.InvariantCulture);
                    NewRecord.AssignedStatus = true;
                    NewRecord.CreationDate = DateTime.Now;
                    NewRecord.CreatedById = Convert.ToInt64(Session["UserID"]);
                    db.GroupCourses.Add(NewRecord);
                    db.SaveChanges();
                }
            }
        }
        #endregion

        #region // Delete Group
        [HttpPost]
        public string DeleteGroup(int id = 0)
        {
            // a group can not be deleted if it is the last group assigned to organisation.
            // a group can not be deleted if it is the last group assigned to user. i.e atleast one group should be selected in organisation.
            var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
            var
                GroupExist = db.Groups.Find(id);
            if (GroupExist != null)
            {
                var OrgCount = db.Groups.Where(x => x.OrganisationID == GroupExist.OrganisationID && GroupExist.IsDeleted == false).Count();
                if (OrgCount == 1)
                    return string.Format(LMSResourse.Admin.Organisation.msgDeleteOrganisation, GroupExist.GroupName);

                var SinglGroupAssignedtoUser = db.Database.SqlQuery<int>(@"Select  COUNT(1) from UserGroup where UserGroup.UserId in ( Select usg.UserId from UserGroup usg where usg.GroupID = " + GroupExist.GroupID + ") group by UserGroup.UserId having COUNT(1) = 1");
                if (SinglGroupAssignedtoUser.Count() > 0)
                    return string.Format(LMSResourse.Admin.Organisation.msgDeleteOrganisation, GroupExist.GroupName);

                GroupExist.IsDeleted = true;
                GroupExist.DeleteInformation = " : " + GroupExist.GroupName + " is delete by userName : " + db.UserProfiles.Find(currentLoginUser).EmailAddress + " on date" + DateTime.Now.ToString();
                db.SaveChanges();
                return "";
            }
            else
                return LMSResourse.Admin.Group.msgInvalidGroup;
            return "";

        }
        #endregion

        #region get organization max user & expiry date
        public ActionResult getOrganizationLicenseDetail(string orgid)
        {
            int OID=int.Parse(orgid);
            var orgDetail = db.Organisations.Where(a => a.OrganisationID ==OID).FirstOrDefault();
            if (orgDetail!=null)
            return Json(new {success=true, maxuser = orgDetail.MaxUsers, expdate = orgDetail.ExpiryDate.Value.ToString("dd/MM/yyyy") }, JsonRequestBehavior.AllowGet);
            else
            return Json(new {success=false, maxuser = 0, expdate = "" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region get group name exists on organisation page
        public int AjaxGroupExists(string gname)
        {
            var checkGroupName = db.Groups.Where(grp => grp.GroupName.TrimEnd().TrimStart().ToLower() == gname.TrimEnd().TrimStart().ToLower()).Select(grp => grp).SingleOrDefault();
            if (checkGroupName != null) // check the duplicate name for group 
            {
                return 1;
            }
            return 0;
        }
        #endregion
    }
}

