using FileManagementAPI.DTOs;

using FileManagementAPI.Models;
using FileManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FileManagementAPI.Controllers
{
    [Authorize]
    public class FilesController : BaseController
    {
        private readonly FileRepository _fileRepository;
        private readonly FolderRepository _folderRepository;
        private readonly UserRepository _userRepository;
        private readonly IWebHostEnvironment _environment;

        public FilesController(
            FileRepository fileRepository,
            FolderRepository folderRepository,
            UserRepository userRepository,
            IWebHostEnvironment environment)
        {
            _fileRepository = fileRepository;
            _folderRepository = folderRepository;
            _userRepository = userRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var files = await _fileRepository.GetPagedFilesByUserIdAsync(userId, page, pageSize);
                var totalCount = await _fileRepository.CountFilesByUserIdAsync(userId);

                var fileDetails = files.Select(file => new FileDetailsDto
                {
                    Id = file.Id,
                    Name = file.Name,
                    OriginalFileName = file.OriginalFileName,
                    ContentType = file.ContentType,
                    Size = file.Size,
                    CreatedAt = file.CreatedAt,
                    UpdatedAt = file.UpdatedAt,
                    UserId = file.UserId,
                    Username = file.User?.UserName ?? string.Empty,
                    FolderId = file.FolderId,
                    FolderName = file.Folder?.Name
                }).ToList();

                return Ok(new FileListDto
                {
                    Files = fileDetails,
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

        [HttpGet("folder/{folderId}")]
        public async Task<IActionResult> GetFilesByFolder(int folderId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var folder = await _folderRepository.GetByIdAsync(folderId);
                if (folder == null)
                    return NotFound(new { Error = "Folder not found" });

                if (folder.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var files = await _fileRepository.GetFilesByFolderIdAsync(folderId);
                var fileDetails = files.Select(file => new FileDetailsDto
                {
                    Id = file.Id,
                    Name = file.Name,
                    OriginalFileName = file.OriginalFileName,
                    ContentType = file.ContentType,
                    Size = file.Size,
                    CreatedAt = file.CreatedAt,
                    UpdatedAt = file.UpdatedAt,
                    UserId = file.UserId,
                    Username = file.User?.UserName ?? string.Empty,
                    FolderId = file.FolderId,
                    FolderName = file.Folder?.Name
                }).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Ok(new FileListDto
                {
                    Files = fileDetails,
                    TotalCount = files.Count(),
                    PageSize = pageSize,
                    CurrentPage = page
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var file = await _fileRepository.GetByIdAsync(id);
                if (file == null)
                    return NotFound(new { Error = "File not found" });

                if (file.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var fileDetails = new FileDetailsDto
                {
                    Id = file.Id,
                    Name = file.Name,
                    OriginalFileName = file.OriginalFileName,
                    ContentType = file.ContentType,
                    Size = file.Size,
                    CreatedAt = file.CreatedAt,
                    UpdatedAt = file.UpdatedAt,
                    UserId = file.UserId,
                    Username = file.User?.UserName ?? string.Empty,
                    FolderId = file.FolderId,
                    FolderName = file.Folder?.Name
                };

                return Ok(fileDetails);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto fileUploadDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (fileUploadDto.File == null || fileUploadDto.File.Length == 0)
                    return BadRequest(new { Error = "No file was uploaded" });

                if (fileUploadDto.FolderId.HasValue)
                {
                    var folder = await _folderRepository.GetByIdAsync(fileUploadDto.FolderId.Value);
                    if (folder == null)
                        return NotFound(new { Error = "Folder not found" });

                    if (folder.UserId != userId && !User.IsInRole("Admin"))
                        return Forbid();
                }

                var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", userId);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileUploadDto.File.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileUploadDto.File.CopyToAsync(stream);
                }

                var fileEntity = new FileEntity
                {
                    Name = Path.GetFileNameWithoutExtension(fileUploadDto.File.FileName),
                    OriginalFileName = fileUploadDto.File.FileName,
                    FilePath = filePath,
                    ContentType = fileUploadDto.File.ContentType,
                    Size = fileUploadDto.File.Length,
                    UserId = userId,
                    FolderId = fileUploadDto.FolderId
                };

                await _fileRepository.AddAsync(fileEntity);
                await _fileRepository.SaveChangesAsync();

                return Ok(new { Message = "File uploaded successfully", FileId = fileEntity.Id });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var file = await _fileRepository.GetByIdAsync(id);
                if (file == null)
                    return NotFound(new { Error = "File not found" });

                if (file.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                if (!System.IO.File.Exists(file.FilePath))
                    return NotFound(new { Error = "File not found on disk" });

                var memory = new MemoryStream();
                using (var stream = new FileStream(file.FilePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, file.ContentType, file.OriginalFileName);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPut("rename/{id}")]
        public async Task<IActionResult> RenameFile(int id, [FromBody] FileRenameDto renameFileDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var file = await _fileRepository.GetByIdAsync(id);
                if (file == null)
                    return NotFound(new { Error = "File not found" });

                if (file.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                file.Name = renameFileDto.NewName;
                file.UpdatedAt = DateTime.UtcNow;

                _fileRepository.Update(file);
                await _fileRepository.SaveChangesAsync();

                return Ok(new { Message = "File renamed successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var file = await _fileRepository.GetByIdAsync(id);
                if (file == null)
                    return NotFound(new { Error = "File not found" });

                if (file.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                if (System.IO.File.Exists(file.FilePath))
                {
                    System.IO.File.Delete(file.FilePath);
                }

                _fileRepository.Remove(file);
                await _fileRepository.SaveChangesAsync();

                return Ok(new { Message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}