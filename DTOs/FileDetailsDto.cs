namespace FileManagementAPI.DTOs
{
    public class FileDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int? FolderId { get; set; }
        public string? FolderName { get; set; }
    }
}