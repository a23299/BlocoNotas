using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Models;

namespace BlocoNotas.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // tabelas DB
        public DbSet<User> User { get; set; }
        public DbSet<Note> Note { get; set; }
        public DbSet<NoteTagged> NoteTagged { get; set; }
        public DbSet<Tag> Tag { get; set; }
    }
}
