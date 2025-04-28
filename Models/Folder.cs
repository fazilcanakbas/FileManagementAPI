namespace FileManagementAPI.Models
{
    public class Folder : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }

        public virtual AppUser? User { get; set; }
        public virtual Folder? ParentFolder { get; set; }
        public virtual ICollection<Folder>? SubFolders { get; set; }
        public virtual ICollection<FileEntity>? Files { get; set; }
    }
}