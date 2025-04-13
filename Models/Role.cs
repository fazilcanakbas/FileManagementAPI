using Microsoft.AspNetCore.Identity;

namespace FileManagementAPI.Models
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}