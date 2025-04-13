using System.ComponentModel.DataAnnotations;

namespace FileManagementAPI.DTOs
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        public string? Description { get; set; }

        public int? FolderId { get; set; }
    }
}