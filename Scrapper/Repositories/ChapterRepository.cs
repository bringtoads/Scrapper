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
            await _context.Chapters.AddAsync(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter is not null)
            {
                _context.Chapters.Remove(chapter);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Chapter>> GetAllAsync()
        {
            return await _context.Chapters.Include(c => c.Novel).ToListAsync();
        }
        public async Task<List<Chapter>> GetChaptersByNovelIdAsync(int novelId)
        {
            return await _context.Chapters
                .Where(c => c.NovelId == novelId)
                .ToListAsync();
        }
        public async Task<Chapter?> GetByIdAsync(int id)
        {
            return await _context.Chapters.Include(c => c.Novel).FirstOrDefaultAsync(c => c.ChapterId == id);
        }

        public async Task UpdateAsync(Chapter chapter)
        {
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
        }
    }
}
