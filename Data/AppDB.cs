using Microsoft.EntityFrameworkCore;
using SOb.Models;

namespace SOb.Data
{
    public class AppDB : DbContext
    {
        public AppDB(DbContextOptions<AppDB> options): base(options) { }
        public DbSet<ValueEntry> Values => Set<ValueEntry>();
        public DbSet<ProcessingResult> results => Set<ProcessingResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ValueEntry>().HasIndex(v => v.fileName);
        }
    }
}
