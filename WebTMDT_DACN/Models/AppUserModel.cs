using Microsoft.AspNetCore.Identity;

namespace WebTMDT_DACN.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occupation { get; set; }
        public string RoleId { get; set; }
        public string Token { get; set; }
    }
}
