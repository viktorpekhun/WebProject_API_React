using Microsoft.EntityFrameworkCore;
using WebProject.Server.Models;

namespace WebProject.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "Bob", Email = "myemail1@gmail.com", Password = "password1", RefreshToken = "token", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(2) },
                new User { Id = 2, Username = "Rick", Email = "myemail2@gmail.com", Password = "password2", RefreshToken = "token", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(2) },
                new User { Id = 3, Username = "John", Email = "myemail3@gmail.com", Password = "password3", RefreshToken = "token", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(2) }
            );
        }
    }
}
