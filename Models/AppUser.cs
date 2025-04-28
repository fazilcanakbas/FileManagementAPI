using Microsoft.AspNetCore.Identity;

namespace FileManagementAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        
        public virtual ICollection<Folder>? Folders { get; set; }
        public virtual ICollection<FileEntity>? Files { get; set; }
    }
}