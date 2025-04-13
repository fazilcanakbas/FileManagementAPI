namespace FileManagementAPI.DTOs
{
    public class UserListDto
    {
        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}