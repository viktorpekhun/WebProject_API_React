using Microsoft.EntityFrameworkCore;
using WebProject_API_React.Server.Models;

namespace WebProject_API_React.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<BackgroundTask> BackgroundTasks { get; set; }
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
