using System.ComponentModel.DataAnnotations;

namespace FileManagementAPI.DTOs
{
    public class RenameFileDto
    {
        [Required]
        public string NewName { get; set; } = string.Empty;
    }
}