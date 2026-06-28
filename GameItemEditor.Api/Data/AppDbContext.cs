using GameItemEditor.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GameItemEditor.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<GameItem> GameItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.Rarity)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.BasePrice)
                    .HasPrecision(18, 2);

                entity.Property(e => e.PropertiesJson)
                    .IsRequired()
                    .HasColumnType("jsonb");

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Rarity);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
