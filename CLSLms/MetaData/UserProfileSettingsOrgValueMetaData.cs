using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CLSLms
{
    class UserProfileSettingsOrgValueMetaData
    {

        [StringLength(200)]
        [Required(ErrorMessageResourceName = "reqProfileValuesTitle", ErrorMessageResourceType = typeof(LMSResourse.Admin.ProfileSettings))]
        [LocalizedDisplayName("fldProfileValuesTitle", NameResourceType = typeof(LMSResourse.Admin.ProfileSettings))]
        public string ProfileValuesTitle { get; set; }
    }

    [MetadataType(typeof(UserProfileSettingsOrgValueMetaData))]
    public partial class UserProfileSettingsOrgValue
    {
        
    }
}
