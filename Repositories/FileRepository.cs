using FileManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FileManagementAPI.Repositories
{
    public class FileRepository : GenericRepository<FileEntity>
    {   
        public FileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<FileEntity?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.Folder)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<FileEntity>> GetFilesByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.Folder)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FileEntity>> GetFilesByFolderIdAsync(int folderId)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.Folder)
                .Where(f => f.FolderId == folderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FileEntity>> GetPagedFilesByUserIdAsync(string userId, int page, int pageSize)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.Folder)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountFilesByUserIdAsync(string userId)
        {
            return await _dbSet.CountAsync(f => f.UserId == userId);
        }

        public async Task<int> CountFilesByFolderIdAsync(int folderId)
        {
            return await _dbSet.CountAsync(f => f.FolderId == folderId);
        }


    }
}