using Microsoft.EntityFrameworkCore;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.DataAccess;

// Kobler entitetene til MariaDB. Hver DbSet blir en tabell.
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ressurs> Ressurser { get; set; }
    public DbSet<Behov> Behov { get; set; }
    
    public DbSet<Matching> Matchinger { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ressurs>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeRessurs).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Kontaktpunkt).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Lastekapasitet).HasPrecision(10, 2);
            entity.Property(e => e.OpprettetTidspunkt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        modelBuilder.Entity<Behov>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeBehov).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Prioritet).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Kontaktpunkt).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OpprettetTidspunkt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        modelBuilder.Entity<Matching>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Behov)
                .WithMany()
                .HasForeignKey(e => e.BehovId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Ressurs)
                .WithMany()
                .HasForeignKey(e => e.RessursId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.OpprettetTidspunkt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
