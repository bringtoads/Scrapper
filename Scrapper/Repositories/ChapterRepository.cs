using Microsoft.EntityFrameworkCore;
using Scrapper.Data;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;

namespace Scrapper.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly AppDbContext _context;
        
        public ChapterRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task AddAsync(Chapter chapter)
        {
            await _context.Set<Chapter>().AddAsync(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var chapter = await _context.Set<Chapter>().FindAsync(id);
            if (chapter is not null)
            {
                _context.Set<Chapter>().Remove(chapter);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Chapter>> GetAllAsync()
        {
            return await _context.Set<Chapter>().Include(c => c.Novel).ToListAsync();
        }

        public async Task<Chapter?> GetByIdAsync(int id)
        {
            return await _context.Set<Chapter>().Include(c => c.Novel).FirstOrDefaultAsync(c => c.ChapterId == id);
        }

        public async Task UpdateAsync(Chapter chapter)
        {
            _context.Set<Chapter>().Update(chapter);
            await _context.SaveChangesAsync();
        }
    }
}
