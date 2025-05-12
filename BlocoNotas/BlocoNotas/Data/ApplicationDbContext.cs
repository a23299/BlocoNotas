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

        modelBuilder.Entity<NoteTag>()
            .HasKey(nt => new { nt.NoteId, nt.TagId });
        
        modelBuilder.Entity<NoteTag>()
            .HasOne(nt => nt.Note)
            .WithMany(n => n.NoteTags)
            .HasForeignKey(nt => nt.NoteId);

        modelBuilder.Entity<NoteTag>()
            .HasOne(nt => nt.Tag)
            .WithMany(t => t.NoteTags)
            .HasForeignKey(nt => nt.TagId);
    }
}