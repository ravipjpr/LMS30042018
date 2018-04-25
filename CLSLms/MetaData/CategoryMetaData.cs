namespace CLSLms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    public class CategoryMetaData
    {
        [StringLength(200)]
        [LocalizedDisplayName("fldCategory", NameResourceType = typeof(LMSResourse.Admin.Category))]
        [Required(ErrorMessageResourceName = "msgReqCategory", ErrorMessageResourceType = typeof(LMSResourse.Admin.Category))]
        public string CategoryName { get; set; }

        [StringLength(400)]
        [LocalizedDisplayName("fldCategoryDescription", NameResourceType = typeof(LMSResourse.Admin.Category))]
        [Required(ErrorMessageResourceName = "msgReqCategoryDescription", ErrorMessageResourceType = typeof(LMSResourse.Admin.Category))]
        public string CategoryDescription { get; set; }

        [LocalizedDisplayName("fldStatus", NameResourceType = typeof(LMSResourse.Admin.Category))]
        public bool Status { get; set; }

    }
    [MetadataType(typeof(CategoryMetaData))]
    public partial class Category
    {

    }
}
