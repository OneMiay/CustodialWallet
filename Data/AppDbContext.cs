using CustodialWallet.Models;
using Microsoft.EntityFrameworkCore;

namespace CustodialWallet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.Balance).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
