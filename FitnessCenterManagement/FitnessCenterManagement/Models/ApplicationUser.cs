using Microsoft.AspNetCore.Identity;

namespace FitnessCenterManagement.Models
{   //...
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
    }
}
