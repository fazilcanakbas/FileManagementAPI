using FileManagementAPI.DTOs;
using FileManagementAPI.Models;
using FileManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;


namespace FileManagementAPI.Controllers
{
    [Authorize]
    public class FoldersController : BaseController
    {
        private readonly FolderRepository _folderRepository;
        private readonly FileRepository _fileRepository;

        public FoldersController(FolderRepository folderRepository, FileRepository fileRepository)
        {
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetFolders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var folders = await _folderRepository.GetPagedFoldersByUserIdAsync(userId, page, pageSize);
                var totalCount = await _folderRepository.CountFoldersByUserIdAsync(userId);

                var folderDetails = await MapFoldersToDto(folders);

                return Ok(new FolderListDto
                {
                    Folders = folderDetails,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet("root")]
        public async Task<IActionResult> GetRootFolders()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var folders = await _folderRepository.GetRootFoldersByUserIdAsync(userId);
                var folderDetails = await MapFoldersToDto(folders);

                return Ok(folderDetails);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetFolder(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var folder = await _folderRepository.GetByIdAsync(id);
                if (folder == null)
                    return NotFound(new { Error = "Folder not found" });

                if (folder.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var folderDetails = await MapFolderToDto(folder);

                return Ok(folderDetails);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet("{id}/subfolders")]
        public async Task<IActionResult> GetSubFolders(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var folder = await _folderRepository.GetByIdAsync(id);
                if (folder == null)
                    return NotFound(new { Error = "Folder not found" });

                if (folder.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var subFolders = await _folderRepository.GetSubFoldersByParentIdAsync(id);
                var folderDetails = await MapFoldersToDto(subFolders);

                return Ok(folderDetails);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderCreateDto folderCreateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (folderCreateDto.ParentFolderId.HasValue)
                {
                    var parentFolder = await _folderRepository.GetByIdAsync(folderCreateDto.ParentFolderId.Value);
                    if (parentFolder == null)
                        return NotFound(new { Error = "Parent folder not found" });

                    if (parentFolder.UserId != userId && !User.IsInRole("Admin"))
                        return Forbid();
                }

                var folder = new Folder
                {
                    Name = folderCreateDto.Name,
                    Description = folderCreateDto.Description,
                    UserId = userId,
                    ParentFolderId = folderCreateDto.ParentFolderId
                };

                await _folderRepository.AddAsync(folder);
                await _folderRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, folder);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolder(int id, [FromBody] FolderCreateDto folderUpdateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var folder = await _folderRepository.GetByIdAsync(id);
                if (folder == null)
                    return NotFound(new { Error = "Folder not found" });

                if (folder.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                if (folderUpdateDto.ParentFolderId.HasValue)
                {
                    var parentFolder = await _folderRepository.GetByIdAsync(folderUpdateDto.ParentFolderId.Value);
                    if (parentFolder == null)
                        return NotFound(new { Error = "Parent folder not found" });

                    if (parentFolder.UserId != userId && !User.IsInRole("Admin"))
                        return Forbid();

                   
                    if (parentFolder.Id == folder.Id)
                        return BadRequest(new { Error = "A folder cannot be its own parent" });
                }

                folder.Name = folderUpdateDto.Name;
                folder.Description = folderUpdateDto.Description;
                folder.ParentFolderId = folderUpdateDto.ParentFolderId;
                folder.UpdatedAt = DateTime.UtcNow;

                _folderRepository.Update(folder);
                await _folderRepository.SaveChangesAsync();

                return Ok(new { Message = "Folder updated successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var folder = await _folderRepository.GetByIdAsync(id);
                if (folder == null)
                    return NotFound(new { Error = "Folder not found" });

                if (folder.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var files = await _fileRepository.GetFilesByFolderIdAsync(id);
                foreach (var fileEntity in files)
                {
                   
                    if (System.IO.File.Exists(fileEntity.FilePath))
                    {
                        System.IO.File.Delete(fileEntity.FilePath);
                    }

                    _fileRepository.Remove(fileEntity);
                }

                await DeleteSubFolders(id);

                _folderRepository.Remove(folder);
                await _folderRepository.SaveChangesAsync();

                return Ok(new { Message = "Folder and all its contents deleted successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        private async Task DeleteSubFolders(int parentId)
        {
            var subFolders = await _folderRepository.GetSubFoldersByParentIdAsync(parentId);

            foreach (var subFolder in subFolders)
            {
                var files = await _fileRepository.GetFilesByFolderIdAsync(subFolder.Id);
                foreach (var fileEntity in files)
                {
                    if (System.IO.File.Exists(fileEntity.FilePath))
                    {
                        System.IO.File.Delete(fileEntity.FilePath);
                    }
                    else
                    {
                   
                        Console.WriteLine($"Dosya yolu geçersiz: {fileEntity.FilePath}");
                    }

                    _fileRepository.Remove(fileEntity);
                }

              
                await DeleteSubFolders(subFolder.Id);

                _folderRepository.Remove(subFolder);
            }
        }

        private async Task<List<FolderDetailsDto>> MapFoldersToDto(IEnumerable<Folder> folders)
        {
            var folderDetailsList = new List<FolderDetailsDto>();

            foreach (var folder in folders)
            {
                var folderDetails = await MapFolderToDto(folder);
                folderDetailsList.Add(folderDetails);
            }

            return folderDetailsList;
        }

        private async Task<FolderDetailsDto> MapFolderToDto(Folder folder)
        {
            int fileCount = folder.Files?.Count ?? await _fileRepository.CountFilesByFolderIdAsync(folder.Id);
            int subFolderCount = folder.SubFolders?.Count ?? await _folderRepository.CountSubFoldersByParentIdAsync(folder.Id);

            return new FolderDetailsDto
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                CreatedAt = folder.CreatedAt,
                UpdatedAt = folder.UpdatedAt,
                UserId = folder.UserId,
                Username = folder.User?.UserName ?? string.Empty,
                ParentFolderId = folder.ParentFolderId,
                ParentFolderName = folder.ParentFolder?.Name,
                FileCount = fileCount,
                SubFolderCount = subFolderCount
            };
        }
    }
}