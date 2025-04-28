namespace FileManagementAPI.Models
{
    public class FileEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? FolderId { get; set; }

        public virtual AppUser? User { get; set; }
        public virtual Folder? Folder { get; set; }
    }
}