using Livros.Entities;

namespace Livros.Models
{
    public class LivrosRequest
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }

        public static implicit operator Livro(LivrosRequest livro)
        {
            return new Livro
            {
                Titulo = livro.Titulo,
                Autor = livro.Autor,
            };
        }
    }
}
