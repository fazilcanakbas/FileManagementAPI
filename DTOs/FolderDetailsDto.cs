namespace FileManagementAPI.DTOs
{
    public class FolderDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
        public string? ParentFolderName { get; set; }
        public int FileCount { get; set; }
        public int SubFolderCount { get; set; }
    }
}