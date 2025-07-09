using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Models;

namespace BlocoNotas.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NoteTag> NoteTags { get; set; }
        public DbSet<NoteShare> NoteShares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

 
    // User entity config
    modelBuilder.Entity<ApplicationUser>()
        .Property(u => u.CreatedAt)
        .HasDefaultValueSql("GETDATE()");

    modelBuilder.Entity<ApplicationUser>()
        .HasIndex(u => u.UserName)
        .IsUnique();

    modelBuilder.Entity<ApplicationUser>()
        .HasIndex(u => u.Email)
        .IsUnique();

    // Note entity config
    modelBuilder.Entity<Note>()
        .Property(n => n.CreatedAt)
        .HasDefaultValueSql("GETDATE()");
        
    // Remove DateTime.Now defaults from model properties if using SQL defaults
    modelBuilder.Entity<Note>()
        .Property(n => n.UpdatedAt)
        .HasDefaultValueSql("GETDATE()");

    modelBuilder.Entity<Note>()
        .HasOne(n => n.User)
        .WithMany(u => u.Notes)
        .HasForeignKey(n => n.UserFK)
        .OnDelete(DeleteBehavior.Cascade);

    // NoteShare entity config
    modelBuilder.Entity<NoteShare>()
        .Property(ns => ns.SharedAt)
        .HasDefaultValueSql("GETDATE()");

    modelBuilder.Entity<NoteShare>()
        .HasOne(ns => ns.Note)
        .WithMany(n => n.SharedWith)
        .HasForeignKey(ns => ns.NoteId)
        .OnDelete(DeleteBehavior.Restrict); // Fixed: Allow cascade from Note

    modelBuilder.Entity<NoteShare>()
        .HasOne(ns => ns.SharedWithUser)
        .WithMany(u => u.SharedWithMe)
        .HasForeignKey(ns => ns.UserShareFK)
        .OnDelete(DeleteBehavior.Cascade); // Keep NoAction to avoid circular cascade

    // NoteTag (join table) entity config
    modelBuilder.Entity<NoteTag>()
        .HasKey(nt => new { nt.NoteTagFK, nt.TagFK });

    modelBuilder.Entity<NoteTag>()
        .HasOne(nt => nt.Note)
        .WithMany(n => n.NoteTags)
        .HasForeignKey(nt => nt.NoteTagFK)
        .OnDelete(DeleteBehavior.Cascade); // Added: Clean up when Note deleted

    modelBuilder.Entity<NoteTag>()
        .HasOne(nt => nt.Tag)
        .WithMany(t => t.NoteTags)
        .HasForeignKey(nt => nt.TagFK)
        .OnDelete(DeleteBehavior.Cascade); // Added: Clean up when Tag deleted
}

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

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
