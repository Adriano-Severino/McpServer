using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Livros.Entities
{
    public class Livro
    {
        public Livro()
        {
            Date = DateTime.Now;
        }

        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public DateTime Date { get; set; }
    }
}
