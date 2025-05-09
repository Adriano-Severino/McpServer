using Livros.Entities;

namespace Livros.Services
{
    public interface ILivroServices
    {
        Task<IEnumerable<Livro>> GetLivrosAsync(string titulo);
        Task<Livro?> GetLivroByIdAsync(int id);
        Task<Livro?> GetLivroByAutorAsync(string autor);
        Task AddLivroAsync(Livro livro);
        Task UpdateLivroAsync(Livro livro);
        Task DeleteLivroAsync(int id);
    }
}
