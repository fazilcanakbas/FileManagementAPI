using System.ComponentModel.DataAnnotations;

namespace FileManagementAPI.DTOs
{
    public class FileRenameDto
    {
        [Required]
        public string NewName { get; set; } = string.Empty;
    }
}