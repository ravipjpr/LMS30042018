using LMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CLSLms;

namespace LMS.Controllers
{
    [CustomAuthorize]
    public class CategoryController : Controller
    {
        private LeopinkLMSDBEntities db = new LeopinkLMSDBEntities();

        #region /// Category listing
        
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult AjaxHandlerCategory(jQueryDataTableParamModel param)
        {
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]); // sort column index
            Func<Category, string> orderingFunction = (c => sortColumnIndex == 0 ? c.CategoryName.TrimEnd().TrimStart().ToLower() :
                                                        sortColumnIndex == 1 ? ((c.CategoryDescription != null) ? c.CategoryDescription.TrimEnd().TrimStart().ToLower() : "-") :                                                        
                                                        sortColumnIndex == 2 ? (c.Status.ToString()) :
                                                        c.CategoryName.ToLower());
            var sortDirection = Request["sSortDir_0"]; // sort column direction
            IEnumerable<Category> filterCategory = null;
            /// search action
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filterCategory = from cat in db.Categories
                                 where cat.CategoryName.ToLower().Contains(param.sSearch.ToLower()) ||
                                 cat.CategoryDescription.ToLower().Contains(param.sSearch.ToLower())
                                 select cat;
            }
            else
            {
                filterCategory = from cat in db.Categories
                                 orderby cat.CategoryName.ToLower() 
                                 select cat;
            }

            // ordering action
            if (sortColumnIndex == 3)
            {
                filterCategory = (sortDirection == "asc") ? filterCategory.OrderBy(grp => grp.CreationDate) : filterCategory.OrderByDescending(grp => grp.CreationDate);
            }
            else
                if (sortDirection == "asc")
                {
                    filterCategory = filterCategory.OrderBy(orderingFunction);
                }
                else if (sortDirection == "desc")
                {
                    filterCategory = filterCategory.OrderByDescending(orderingFunction);
                }

            filterCategory = filterCategory.Where(x=>x.IsDeleted == false).ToList();

            // records to display            
            var displayedCategory = filterCategory.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            if (param.iDisplayLength == -1)
                displayedCategory = filterCategory;
            var ActiveStatus = LMSResourse.Common.Common.lblActiveStatus;
            var InactiveStatus = LMSResourse.Common.Common.lblInactiveStatus;
            var result = from obj in displayedCategory.ToList()
                         select new[] {
                             obj.CategoryName,
                              ((obj.CategoryDescription==null)?"-": obj.CategoryDescription),                              
                              ((obj.Status)?ActiveStatus :InactiveStatus ),
                              string.Format("{0:dd/MM/yyyy HH:mm}",obj.CreationDate),
                               Convert.ToString(obj.CategoryId)
                          };
            //return json data.
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = filterCategory.Count(),
                iTotalDisplayRecords = filterCategory.Count(),
                aaData = result
            },
                           JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region // Create Category
        
        public ActionResult CreateCategory()
        {
            var model = new CLSLms.Category();
            model.Status = true;
            return View(model);
            
        }

        /// <summary>
        /// create category http post method
        /// </summary>
        /// <param name="ObjCategory"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateCategory(Category ObjCategory)
        {
            if (ModelState.IsValid) // check the model is validate or not.
            {
                var checkCategoryName = db.Categories.Where(cat => cat.IsDeleted == false && cat.CategoryName.TrimEnd().TrimStart().ToLower() == ObjCategory.CategoryName.TrimEnd().TrimStart().ToLower()).Select(cat => cat).SingleOrDefault();
                if (checkCategoryName == null) // find the category name already exist and not deleted.
                {
                    // save the category object in database.
                    ObjCategory.CreatedById = Convert.ToInt64(Session["UserID"]);
                    ObjCategory.CreationDate = DateTime.Now;
                    ObjCategory.IsDeleted = false;
                    db.Categories.Add(ObjCategory);
                    db.SaveChanges(); // save in database

                    var defaultLanguageId = db.InstanceInfoes.Find(1).DefaultLanguage; // check the default language of project

                    CategoryInfo ObjCatInfo = new CategoryInfo(); // create object of category info table to save the record with language information
                    ObjCatInfo.CategoryId = ObjCategory.CategoryId;
                    ObjCatInfo.CategoryName = ObjCategory.CategoryName;
                    ObjCatInfo.CategoryDescription = ObjCategory.CategoryDescription;
                    ObjCatInfo.LanguageId = defaultLanguageId;
                    ObjCatInfo.CreatedById = Convert.ToInt64(Session["UserID"]);
                    ObjCatInfo.CreationDate = DateTime.Now;
                    db.CategoryInfoes.Add(ObjCatInfo);
                    db.SaveChanges(); // save data in category info table in database

                    return RedirectToAction("Index", "Category"); // redirect to category index page.
                }
                else
                {
                    ModelState.AddModelError("CategoryName", LMSResourse.Admin.Category.msgDupCategoryName); // if error exist in model then return the model with error messages.
                }
            }
            return View(ObjCategory);
        }

        #endregion

        #region // Edit Category
        
        public ActionResult EditCategory(int id=0)
        {
            var ObjCategory = db.Categories.Find(id);
            if (ObjCategory == null)
            {
                return HttpNotFound();
            }
            else if (ObjCategory.IsDeleted == false)
                return View(ObjCategory);
            else
                return RedirectToAction("Index", "Category");
        }

        /// <summary>
        /// http post method of Editcategory.
        /// </summary>
        /// <param name="ObjCategory"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditCategory(Category ObjCategory)
        {
            if (ModelState.IsValid)
            {
                var duplicateTile = db.Categories.Where(cat => cat.IsDeleted == false && cat.CategoryName == ObjCategory.CategoryName && cat.CategoryId != ObjCategory.CategoryId).FirstOrDefault();
                // check duplicate title
                if (duplicateTile == null)
                {
                    // find the categories
                    var dbObjCategory = db.Categories.Find(ObjCategory.CategoryId);
                    // if record exist then update the record.
                    if (dbObjCategory != null) 
                    {
                        dbObjCategory.CategoryName = ObjCategory.CategoryName;
                        dbObjCategory.CategoryDescription = ObjCategory.CategoryDescription;
                        dbObjCategory.Status = ObjCategory.Status;
                        dbObjCategory.DateLastModified = DateTime.Now;
                        dbObjCategory.LastModifiedById = Convert.ToInt64(Session["UserID"]);
                        db.SaveChanges();

                        var defaultLanguageId = db.InstanceInfoes.Find(1).DefaultLanguage;

                        var dbObjCategoryInfo = db.CategoryInfoes.Where(catinfo => catinfo.CategoryId == dbObjCategory.CategoryId && catinfo.LanguageId == defaultLanguageId).FirstOrDefault();
                        // if category info record not exist the create a new record.
                        if (dbObjCategoryInfo == null) 
                        {
                            CategoryInfo ObjCatInfo = new CategoryInfo();
                            ObjCatInfo.CategoryId = ObjCategory.CategoryId;
                            ObjCatInfo.CategoryName = ObjCategory.CategoryName;
                            ObjCatInfo.CategoryDescription = ObjCategory.CategoryDescription;
                            ObjCatInfo.LanguageId = defaultLanguageId;
                            ObjCategory.LastModifiedById = Convert.ToInt64(Session["UserID"]);
                            ObjCategory.DateLastModified = DateTime.Now;
                            db.SaveChanges();
                        }
                        // update the existing category info record.
                        else
                        {
                            dbObjCategoryInfo.CategoryName = dbObjCategory.CategoryName;
                            dbObjCategoryInfo.CategoryDescription = dbObjCategory.CategoryDescription;
                            dbObjCategoryInfo.LanguageId = defaultLanguageId;
                            dbObjCategoryInfo.DateLastModified = DateTime.Now;
                            dbObjCategoryInfo.LastModifiedById = Convert.ToInt64(Session["UserID"]);
                            db.SaveChanges();
                        }
                        return RedirectToAction("Index", "Category");
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                else
                {
                    // return model with error message.
                    ModelState.AddModelError("CategoryName", LMSResourse.Admin.Category.msgDupCategoryName);
                }
            }
            return View(ObjCategory);
        }
        #endregion
        #region // Delete category
        [HttpPost]
        public string DeleteCategory(int id = 0)
        {
            var currentLoginUser = Convert.ToInt64(Session["UserID"].ToString());
            var CatExist = db.Categories.Find(id);
            if (CatExist != null)
            {
                var CourseLink = from x in db.Categories
                              join y in db.Courses on x.CategoryId equals y.CategoryId
                              where x.CategoryId == CatExist.CategoryId
                              select x;
                if (CourseLink.Count() == 0)
                {

                    CatExist.IsDeleted = true;
                    CatExist.DeleteInformation = " : " + CatExist.CategoryName + " is delete by userName : " + db.UserProfiles.Find(currentLoginUser).EmailAddress + " on date" + DateTime.Now.ToString();
                    db.SaveChanges();
                }
                else
                {
                    return string.Format(LMSResourse.Admin.Category.msgDeleteCategory, CatExist.CategoryName);
                }
            }
            else
                return LMSResourse.Admin.Category.msgInvalidCategory;
            return "";

        }
        #endregion
    }
}