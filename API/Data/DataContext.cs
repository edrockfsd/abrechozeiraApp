using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Data
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<ABrechozeiraApp.Models.NivelAcesso> NivelAcesso { get; set; } = default!;
        public DbSet<ABrechozeiraApp.Models.Pessoa> Pessoa { get; set; } = default!;
        public DbSet<ABrechozeiraApp.Models.PessoaCategoria> PessoaCategoria { get; set; } = default!;
        public DbSet<ABrechozeiraApp.Models.PessoaTipo> PessoaTipo { get; set; } = default!;
        public DbSet<ABrechozeiraApp.Models.Produto> Produto { get; set; } = default!;
        public DbSet<ABrechozeiraApp.Models.Usuario> Usuario { get; set; } = default!;
        public DbSet<ABrechozeiraApp.Models.Venda> Venda { get; set; } = default!;

    }
}
