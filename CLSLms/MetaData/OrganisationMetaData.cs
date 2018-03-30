
namespace CLSLms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    public class OrganisationMetaData
    {
        [LocalizedDisplayName("fldOrganisationID", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(50, ErrorMessageResourceName = "msgMxlOrganisationID", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationID", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(50)]
        public int OrganisationUID { get; set; }

        [LocalizedDisplayName("fldOrganisationName", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string OrganisationName { get; set; }

        [LocalizedDisplayName("fldOrganisationlogo", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationlogo", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        public string OrganisationLogo { get; set; }

        [LocalizedDisplayName("fldOrganisationbanner", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        public string OrganisationBanner { get; set; }

        [LocalizedDisplayName("fldOrganisationStatus", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        public bool Status { get; set; }

        [LocalizedDisplayName("fldOrganisationAddress1", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationAddress1", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string AddressLine1 { get; set; }

        [LocalizedDisplayName("fldOrganisationAddress2", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationAddress2", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string AddressLine2 { get; set; }

        [LocalizedDisplayName("fldOrganisationAddress3", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationAddress3", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string AddressLine3 { get; set; }

        [LocalizedDisplayName("fldOrganisationPostalCode", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(10, ErrorMessageResourceName = "msgMxlOrganisationPostalCode", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(10)]
        public string PostalCode { get; set; } 

        [LocalizedDisplayName("fldOrganisationCountry", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(50, ErrorMessageResourceName = "msgReqOrganisationCountry", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(50)]
        public string Country { get; set; }

    }

    //[MetadataType(typeof(OrganisationMetaData))]
    //public partial class Organisation
    //{
    //}

    public class _Organisation
    {
        public int OrganisationID { get; set; }

        [LocalizedDisplayName("fldOrganisationID", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(50, ErrorMessageResourceName = "msgMxlOrganisationID", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationID", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(50)]
        public string OrganisationUID { get; set; }

        [LocalizedDisplayName("fldOrganisationName", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string OrganisationName { get; set; }

        [LocalizedDisplayName("fldOrganisationlogo", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationlogo", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        public string OrganisationLogo { get; set; }

        [LocalizedDisplayName("fldOrganisationbanner", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqOrganisationName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        public string OrganisationBanner { get; set; }

        [LocalizedDisplayName("fldOrganisationStatus", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        public bool Status { get; set; }

        [LocalizedDisplayName("fldOrganisationAddress1", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationAddress1", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string AddressLine1 { get; set; }

        [LocalizedDisplayName("fldOrganisationAddress2", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationAddress2", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string AddressLine2 { get; set; }

        [LocalizedDisplayName("fldOrganisationAddress3", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(100, ErrorMessageResourceName = "msgMxlOrganisationAddress3", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(100)]
        public string AddressLine3 { get; set; }

        [LocalizedDisplayName("fldOrganisationPostalCode", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(10, ErrorMessageResourceName = "msgMxlOrganisationPostalCode", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(10)]
        public string PostalCode { get; set; }

        [LocalizedDisplayName("fldOrganisationCountry", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [MaxLength(50, ErrorMessageResourceName = "msgReqOrganisationCountry", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [StringLength(50)]
        public string Country { get; set; }

        [LocalizedDisplayName("thNoOfUsers", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        [Required(ErrorMessageResourceName = "msgReqLicMaxUsers", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessageResourceName = "msgStrMaxUsersNumeric", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        //[Range(0, int.MaxValue, ErrorMessageResourceName = "msgErrorIntegerOnly", ErrorMessageResourceType = typeof(LMSResourse.Admin.Group))]
        public long MaxUsers { get; set; }

        [LocalizedDisplayName("lblExpDate", NameResourceType = typeof(LMSResourse.Admin.Group))]
        public Nullable<System.DateTime> ExpiryDate { get; set; }

        [LocalizedDisplayName("lblExpDate", NameResourceType = typeof(LMSResourse.Admin.Group))]
        [Required(ErrorMessageResourceName = "msgReqLicExpDate", ErrorMessageResourceType = typeof(LMSResourse.Admin.Organisation))]
        public string ExpDate { get; set; }

        public string DateFormatForClientSide { get; set; }

        [LocalizedDisplayName("lblAllowIndividualAssignment", NameResourceType = typeof(LMSResourse.Admin.Organisation))]
        public bool IsUserAssignment { get; set; }

        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<License> Licenses { get; set; }
        public virtual ICollection<OrganisationInfo> OrganisationInfoes { get; set; }
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
        public virtual ICollection<UserProfileSettingsOrg> UserProfileSettingsOrgs { get; set; }
    }
}
