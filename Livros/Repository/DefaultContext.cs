using Livros.Entities;
using Microsoft.EntityFrameworkCore;

namespace Livros.Repository
{
    public class DefaultContext : DbContext
    {
        public DbSet<Livro> Livros { get; set; }
        public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
        {
        }
    }
}
