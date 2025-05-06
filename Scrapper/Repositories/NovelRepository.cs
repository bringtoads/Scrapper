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
             return await _context.Set<Novel>().Include(n => n.Author).FirstOrDefaultAsync(n => n.NovelId == id); 
        }

        public async Task<IEnumerable<Novel>> GetAllAsync()
        {
            return await _context.Set<Novel>().Include(n => n.Author).ToListAsync();
        }
        public async Task<Novel?> GetBySourceUrlAsync(string sourceUrl)
        {
            return await _context.Novels
                .FirstOrDefaultAsync(n => n.SourceUrl == sourceUrl);
        }
        public async Task AddAsync(Novel novel)
        {
            await _context.Set<Novel>().AddAsync(novel);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Novel novel)
        {
            _context.Set<Novel>().Update(novel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var novel = await _context.Set<Novel>().FindAsync(id);
            if (novel is not null)
            {
                _context.Set<Novel>().Remove(novel);
                await _context.SaveChangesAsync();
            }
        }
    }
}
