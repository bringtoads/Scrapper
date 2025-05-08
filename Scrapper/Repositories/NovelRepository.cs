using Microsoft.EntityFrameworkCore;
using Scrapper.Data;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;

namespace Scrapper.Repositories
{
    public class NovelRepository : INovelRepository
    {
        private readonly AppDbContext _context;
        public NovelRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Novel?> GetByIdAsync(int id)
        {
             return await _context.Novels.Include(n => n.Author).FirstOrDefaultAsync(n => n.NovelId == id); 
        }

        public async Task<List<Novel?>> GetNovelsByAuthor(string name)
        {
            await Task.CompletedTask;
            return null;
        }
        public async Task<IEnumerable<Novel>> GetAllAsync()
        {
            return await _context.Novels.Include(n => n.Author).ToListAsync();
        }
        public async Task<Novel?> GetBySourceUrlAsync(string sourceUrl)
        {
            return await _context.Novels
                .FirstOrDefaultAsync(n => n.SourceUrl == sourceUrl);
        }
        public async Task AddAsync(Novel novel)
        {
            await _context.Novels.AddAsync(novel);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Novel novel)
        {
            _context.Novels.Update(novel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var novel = await _context.Novels.FindAsync(id);
            if (novel is not null)
            {
                _context.Novels.Remove(novel);
                await _context.SaveChangesAsync();
            }
        }
    }
}
