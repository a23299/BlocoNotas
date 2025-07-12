using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Models;

namespace BlocoNotas.Data
{
    /// <summary>
    /// Contexto de banco de dados da aplicação, incluindo identidade e entidades personalizadas.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Construtor que recebe as opções do contexto.
        /// </summary>
        /// <param name="options">Opções de configuração do DbContext</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet para a entidade Note (Notas).
        /// </summary>
        public DbSet<Note> Notes { get; set; }

        /// <summary>
        /// DbSet para a entidade Tag (Tags).
        /// </summary>
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// DbSet para a entidade NoteTag (tabela de relação entre Notas e Tags).
        /// </summary>
        public DbSet<NoteTag> NoteTags { get; set; }

        /// <summary>
        /// DbSet para a entidade NoteShare (compartilhamento de notas).
        /// </summary>
        public DbSet<NoteShare> NoteShares { get; set; }

        /// <summary>
        /// Configurações e restrições das entidades e relacionamentos no modelo.
        /// </summary>
        /// <param name="modelBuilder">Construtor do modelo do Entity Framework</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações para a entidade ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configurações para a entidade Note
            modelBuilder.Entity<Note>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Note>()
                .Property(n => n.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserFK)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurações para a entidade NoteShare
            modelBuilder.Entity<NoteShare>()
                .Property(ns => ns.SharedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<NoteShare>()
                .HasOne(ns => ns.Note)
                .WithMany(n => n.SharedWith)
                .HasForeignKey(ns => ns.NoteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NoteShare>()
                .HasOne(ns => ns.SharedWithUser)
                .WithMany(u => u.SharedWithMe)
                .HasForeignKey(ns => ns.UserShareFK)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurações para a entidade NoteTag (tabela associativa)
            modelBuilder.Entity<NoteTag>()
                .HasKey(nt => new { nt.NoteTagFK, nt.TagFK });

            modelBuilder.Entity<NoteTag>()
                .HasOne(nt => nt.Note)
                .WithMany(n => n.NoteTags)
                .HasForeignKey(nt => nt.NoteTagFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NoteTags)
                .HasForeignKey(nt => nt.TagFK)
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <summary>
        /// Sobrescreve o método SaveChanges para atualizar timestamps antes de salvar.
        /// </summary>
        /// <returns>Número de objetos afetados no banco de dados</returns>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Sobrescreve o método assíncrono SaveChangesAsync para atualizar timestamps antes de salvar.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Número de objetos afetados no banco de dados</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Atualiza os campos de timestamp (CreatedAt, UpdatedAt, SharedAt) nas entidades antes de salvar.
        /// </summary>
        private void UpdateTimestamps()
        {
            var now = DateTime.Now;

            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.Entity)
                {
                    case Note note:
                        if (entry.State == EntityState.Added)
                        {
                            entry.Property("CreatedAt").CurrentValue = now;
                            entry.Property("UpdatedAt").CurrentValue = now;
                        }
                        else if (entry.State == EntityState.Modified)
                        {
                            entry.Property("UpdatedAt").CurrentValue = now;
                        }
                        break;

                    case ApplicationUser user when entry.State == EntityState.Added:
                        entry.Property("CreatedAt").CurrentValue = now;
                        break;

                    case NoteShare noteShare when entry.State == EntityState.Added:
                        entry.Property("SharedAt").CurrentValue = now;
                        break;
                }
            }
        }
    }
}
