using Microsoft.EntityFrameworkCore;
using Scrapper.Data;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;

namespace Scrapper.Repositories
{
    internal class AuthorRepository : IAuthorRepository
    {
        private readonly AppDbContext _context;

        public AuthorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _context.Authors.ToListAsync();
        }

        public async Task<Author?> GetByIdAsync(int id)
        {
            return await _context.Authors.FindAsync(id);
        }

        public async Task<Author?> GetByNameAsync(string name)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a => a.Name == name);
        }
        public async Task AddAsync(Author author)
        {
            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Author author)
        {
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
            }
        }
    }

}
