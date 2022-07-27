using Microsoft.EntityFrameworkCore;

namespace AuthFirst.Data
{
    public class AuthDbContext:DbContext
    {
        public DbSet<AppUser> AppUsers { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(x => x.UserId);
                entity.Property(x => x.UserId);
                entity.Property(x => x.Provider).HasMaxLength(250);
                entity.Property(x => x.NameIdentifier).HasMaxLength(250);
                entity.Property(x => x.Username).HasMaxLength(250);
                entity.Property(x => x.Password).HasMaxLength(250);
                entity.Property(x => x.Firstname).HasMaxLength(250);
                entity.Property(x => x.Lastname).HasMaxLength(250);
                entity.Property(x => x.Email).HasMaxLength(250);
                entity.Property(x => x.Mobile).HasMaxLength(250);
                entity.Property(x => x.Roles).HasMaxLength(1000);


                entity.HasData(new AppUser
                {
                    Provider = "Cookies",
                    UserId = 1,
                    Email = "ikayode007@gmail.com",
                    Username = "kayode",
                    Password = "love",
                    Firstname = "Oluwakayode",
                    Lastname = "Isaac",
                    Mobile = "07080401503",
                    Roles = "Admin",
                });
            });
        }
    }
}
