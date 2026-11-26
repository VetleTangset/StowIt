using Microsoft.EntityFrameworkCore;

namespace StowIt.Core
{
    public class StowItDbContext : DbContext
    {
        public StowItDbContext(DbContextOptions<StowItDbContext> options) : base(options) { }

        public DbSet<Box> Boxes => Set<Box>();
        public DbSet<Item> Items => Set<Item>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Box>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.Property(x => x.Label).IsRequired();
                b.Property(x => x.Location).HasConversion<string>();
            });

            modelBuilder.Entity<Item>(i =>
            {
                i.Property(x => x.Id).ValueGeneratedNever();
                i.Property(x => x.Name).IsRequired();
            });
        }
    }
}
