namespace FileManagementAPI.DTOs
{
    public class FolderListDto
    {
        public List<FolderDetailsDto> Folders { get; set; } = new List<FolderDetailsDto>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}