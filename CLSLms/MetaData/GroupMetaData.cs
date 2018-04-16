

namespace CLSLms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    public class GroupMetaData
    {
        public int GroupID { get; set; }

        [LocalizedDisplayName("fldGroupName", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlGroupName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [Required(ErrorMessageResourceName = "msgReqGroupName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(100)]
        public string GroupName { get; set; }

        [LocalizedDisplayName("flsGroupDesc", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(400, ErrorMessageResourceName = "msgMxlGroupDesc", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [Required(ErrorMessageResourceName = "msgReqGroupDesc", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(400)]
        public string GroupDescription { get; set; }

        [LocalizedDisplayName("fldGroupManagerName", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlGroupManager", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(100)]
        public string GroupManager { get; set; }

        [LocalizedDisplayName("fldGroupEmailId", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(256, ErrorMessageResourceName = "msgMxlGroupEmailId", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(256)]
        [EmailAddress(ErrorMessage = "", ErrorMessageResourceName = "msgValidEmailAddress", ErrorMessageResourceType = typeof(LMSResourse.Admin.User))]
        public string EmailAddress { get; set; }

        [LocalizedDisplayName("fldGroupContactNo", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(50, ErrorMessageResourceName = "msgMxlGroupContactNo", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [LocalizedDisplayName("fldStatus", NameResourceType = typeof(LMSResourse.Admin.Group))]
        public bool Status { get; set; }

        [LocalizedDisplayName("fldOrganisationID", NameResourceType = typeof(LMSResourse.Admin.Group))]
        public Nullable<int> OrganisationID { get; set; }

        public long MaxUsers { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }


    }

    //[MetadataType(typeof(GroupMetaData))]
    //public partial class Group
    //{
    //}


    public class _Group {

        public int GroupID { get; set; }

        [LocalizedDisplayName("fldGroupName", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlGroupName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [Required(ErrorMessageResourceName = "msgReqGroupName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(100)]
        public string GroupName { get; set; }

        [LocalizedDisplayName("flsGroupDesc", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(400, ErrorMessageResourceName = "msgMxlGroupDesc", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [Required(ErrorMessageResourceName = "msgReqGroupDesc", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(400)]
        public string GroupDescription { get; set; }

        [LocalizedDisplayName("fldGroupManagerName", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlGroupManager", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(100)]
        public string GroupManager { get; set; }

        [LocalizedDisplayName("fldGroupEmailId", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(256, ErrorMessageResourceName = "msgMxlGroupEmailId", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(256)]
        [EmailAddress(ErrorMessage = "", ErrorMessageResourceName = "msgValidEmailAddress", ErrorMessageResourceType = typeof(LMSResourse.Admin.User))]
        public string EmailAddress { get; set; }

        [LocalizedDisplayName("fldGroupContactNo", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [MaxLength(50, ErrorMessageResourceName = "msgMxlGroupContactNo", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [LocalizedDisplayName("fldStatus", NameResourceType = typeof(LMSResourse.Admin.Group))]
        public bool Status { get; set; }

        [LocalizedDisplayName("fldOrganisationID", NameResourceType = typeof(LMSResourse.Admin.Group))]
        public Nullable<int> OrganisationID { get; set; }

        [LocalizedDisplayName("thNoOfUsers", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
       // [Required(ErrorMessageResourceName = "msgReqMaxUser", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessageResourceName = "msgStrMaxUsersNumeric", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        public long MaxUsers { get; set; }

        [LocalizedDisplayName("lblExpDate", NameResourceType = typeof(LMSResourse.Admin.Group))]
        public Nullable<System.DateTime> ExpiryDate { get; set; }

        [LocalizedDisplayName("lblExpDate", NameResourceType = typeof(LMSResourse.Admin.Group))]
        public string ExpDate { get; set; }

        public string DateFormatForClientSide { get; set; }
        public Nullable<long> AssignedUsers { get; set; }
    }
}
