

namespace CLSLms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    public class CourseMetaData
    {
        [StringLength(200)]
        [LocalizedDisplayName("fldCourseName", NameResourceType = typeof(LMSResourse.Admin.Course))]
        [Required(ErrorMessageResourceName = "msgReqCourseName", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]
        public string CourseName { get; set; }

        [StringLength(400)]
        [LocalizedDisplayName("fldDescription", NameResourceType = typeof(LMSResourse.Admin.Course))]
        [Required(ErrorMessageResourceName = "msgReqCourseDesc", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]
        public string CourseDescription { get; set; }

        [Required(ErrorMessageResourceName = "msgReqCourseCategory", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]
        [LocalizedDisplayName("fldCategory", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public int CategoryId { get; set; }

        [LocalizedDisplayName("fldCertificate", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public Nullable<int> CertificateId { get; set; }

        [LocalizedDisplayName("fldCourseFileUrl", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public string FolderLocation { get; set; }

        [LocalizedDisplayName("fldStatus", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public bool Status { get; set; }

        [LocalizedDisplayName("fldScoLabel", NameResourceType = typeof(LMSResourse.Admin.Course))]
        [MaxLength(10, ErrorMessageResourceName = "msgMxlScoLabel", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]
        [StringLength(10)]
        public string ScoLabel { get; set; }

        [LocalizedDisplayName("fldWindowwidth", NameResourceType = typeof(LMSResourse.Admin.Course))]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessageResourceName = "msgStrWinWidthNumeric", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]
        [Range(0, 2000, ErrorMessageResourceName = "msgRngWindowwidth", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]

        public Nullable<int> WindowWidth { get; set; }

        [LocalizedDisplayName("fldWindowheight", NameResourceType = typeof(LMSResourse.Admin.Course))]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessageResourceName = "msgStrWinHeightNumeric", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]
        [Range(0, 2000, ErrorMessageResourceName = "msgRngWindowheight", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]

        public Nullable<int> WindowHeight { get; set; }

        [LocalizedDisplayName("fldPassMarks", NameResourceType = typeof(LMSResourse.Admin.Course))]
        [Range(0, 100, ErrorMessageResourceName = "msgRngPassMarks", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessageResourceName = "msgStrPassMarksNumeric", ErrorMessageResourceType = typeof(LMSResourse.Admin.Course))]

        public int PassMarks { get; set; }

        [LocalizedDisplayName("fldUserDefined", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public Nullable<bool> IsUserDefined { get; set; }

        [LocalizedDisplayName("fldIsMobile", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public bool IsMobile { get; set; } = false;
        [LocalizedDisplayName("fldMandatory", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public bool Mandaotry { get; set; }
        [LocalizedDisplayName("fldCourseDurationInMin", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public int CourseDurationMin { get; set; }
        [LocalizedDisplayName("fldCourseFees", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public int CourseFees { get; set; }
        [LocalizedDisplayName("fldCourseFeeStatus", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public byte FeeType { get; set; }
        [LocalizedDisplayName("fldCourseType", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public Nullable<int> CourseType { get; set; }

        [StringLength(250)]
        [LocalizedDisplayName("fldTags", NameResourceType = typeof(LMSResourse.Admin.Course))]
        public string Tags { get; set; }

    }

    [MetadataType(typeof(CourseMetaData))]
    public partial class Course
    {

    }
}
