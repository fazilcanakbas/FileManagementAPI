using System.ComponentModel.DataAnnotations;

namespace FileManagementAPI.DTOs
{
    public class FolderCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? ParentFolderId { get; set; }
    }
}