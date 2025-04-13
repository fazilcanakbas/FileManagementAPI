using FileManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FileManagementAPI.Repositories
{
    public class FolderRepository : GenericRepository<Folder>
    {
        public FolderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Folder?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.ParentFolder)
                .Include(f => f.Files)
                .Include(f => f.SubFolders)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Folder>> GetFoldersByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.ParentFolder)
                .Include(f => f.Files)
                .Include(f => f.SubFolders)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Folder>> GetRootFoldersByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.Files)
                .Include(f => f.SubFolders)
                .Where(f => f.UserId == userId && f.ParentFolderId == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Folder>> GetSubFoldersByParentIdAsync(int parentId)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.Files)
                .Include(f => f.SubFolders)
                .Where(f => f.ParentFolderId == parentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Folder>> GetPagedFoldersByUserIdAsync(string userId, int page, int pageSize)
        {
            return await _dbSet
                .Include(f => f.User)
                .Include(f => f.ParentFolder)
                .Include(f => f.Files)
                .Include(f => f.SubFolders)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountFoldersByUserIdAsync(string userId)
        {
            return await _dbSet.CountAsync(f => f.UserId == userId);
        }

        public async Task<int> CountSubFoldersByParentIdAsync(int parentId)
        {
            return await _dbSet.CountAsync(f => f.ParentFolderId == parentId);
        }
    }
}