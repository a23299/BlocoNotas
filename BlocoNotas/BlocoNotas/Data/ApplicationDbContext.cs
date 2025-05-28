using BlocoNotas.Models;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<NoteTag> NoteTags { get; set; }
    public DbSet<NoteShare> NoteShares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<Note>()
            .Property(n => n.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Note>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserFK);
            //.OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<NoteTag>()
            .HasKey(nt => new { nt.NoteTagFK, nt.TagFK });

        modelBuilder.Entity<NoteTag>()
            .HasOne(nt => nt.Note)
            .WithMany(n => n.NoteTags)
            .HasForeignKey(nt => nt.NoteTagFK);

        modelBuilder.Entity<NoteTag>()
            .HasOne(nt => nt.Tag)
            .WithMany(t => t.NoteTags)
            .HasForeignKey(nt => nt.TagFK);
        
        //modelBuilder.Entity<NoteShare>()
        //    .HasOne(ns => ns.SharedWithUser)
        //    .WithMany()
        //    .HasForeignKey(ns => ns.SharedWithUserId)
        //    .OnDelete(DeleteBehavior.Restrict); // <-- ON DELETE NO ACTION
        
        //modelBuilder.Entity<NoteShare>()
        //    .HasOne(ns => ns.Note)
        //    .WithMany()
        //    .HasForeignKey(ns => ns.NoteId)
        //    .OnDelete(DeleteBehavior.Restrict); // <-- ON DELETE NO ACTION
        
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction; // <-- ON DELETE NO ACTION
        }
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
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Note)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.Now;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                }
            }

            if (entry.Entity is User && entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.Now;
            }

            if (entry.Entity is NoteShare && entry.State == EntityState.Added)
            {
                entry.Property("SharedAt").CurrentValue = DateTime.Now;
            }
        }
    }
}
