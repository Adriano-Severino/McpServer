using Livros.Entities;
using Livros.Repository;
using Microsoft.EntityFrameworkCore;

namespace Livros.Services
{
    public class LivroServices : ILivroServices
    {
        private readonly DefaultContext _context;
        public LivroServices(DefaultContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Livro>> GetLivrosAsync(string? titulo = null)
        {
           return await _context.Livros
                .AsNoTracking()
                .Where(l => string.IsNullOrEmpty(titulo) || l.Titulo.Contains(titulo))
                .ToListAsync();
        }
        public async Task<Livro?> GetLivroByIdAsync(int id)
        {
            return await _context.Livros.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Livro?> GetLivroByAutorAsync(string autor)
        {
            return await _context.Livros.AsNoTracking().FirstOrDefaultAsync(x => x.Autor == autor);
        }
        public async Task AddLivroAsync(Livro livro)
        {
            await _context.Livros.AddAsync(livro);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLivroAsync(Livro livro)
        {
            _context.Livros.Update(livro);
            await _context.SaveChangesAsync();
        }

        public Task DeleteLivroAsync(int id)
        {
            return _context.Livros
                .Where(l => l.Id == id)
                .ExecuteDeleteAsync();
        }
    }
}
