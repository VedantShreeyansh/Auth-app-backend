using Microsoft.EntityFrameworkCore;
using auth_app_backend.Model;

namespace auth_app_backend.Data
{
    public class NoticeBoardContext : DbContext
    {
        public NoticeBoardContext(DbContextOptions<NoticeBoardContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; } // Added DbSet for User

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .ToTable("messages"); // Specify the correct table name for Messages

            modelBuilder.Entity<User>() // Configuring the User entity
                .ToTable("users"); // Specify the correct table name for Users

            base.OnModelCreating(modelBuilder);
        }
    }
}
