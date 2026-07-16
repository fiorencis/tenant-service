using Microsoft.EntityFrameworkCore;
using TenantService.Domain.Entities;
using TenantService.Domain.Enums;

namespace TenantService.Infrastructure;


public class TenantDbContext : DbContext
{
     public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

     public DbSet<User> Users => Set<User>();
     
     public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

     public DbSet<DbUpdate> DbUpdates => Set<DbUpdate>();

     protected override void OnModelCreating(ModelBuilder modelBuilder)
     {
          modelBuilder.Entity<User>()
               .ToTable("user", "infra") 
               .HasKey(u => u.Id).HasName("user_pkey"); 

          modelBuilder.Entity<User>()
               .HasIndex(u => u.Username)
               .IsUnique();

          modelBuilder.Entity<User>()
               .Property(u => u.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .IsRequired();

          modelBuilder.Entity<User>()
               .Property(u => u.Username)
               .HasColumnName("username") 
               .HasColumnType("varchar(32)")
               .IsRequired();

          modelBuilder.Entity<User>()
               .Property(u => u.FullName)
               .HasColumnName("fullname")
               .HasColumnType("varchar(256)")
               .IsRequired(); 

          modelBuilder.Entity<User>()
               .Property(u => u.Email)
               .HasColumnName("email")
               .HasColumnType("varchar(256)")
               .IsRequired();

          modelBuilder.Entity<User>().Property(u => u.PasswordHash)
               .HasColumnName("passwordhash")
               .HasColumnType("varchar(256)");

          modelBuilder.Entity<User>()
               .Property(u => u.Admin)
               .HasColumnName("admin")
               .HasColumnType("boolean")
               .IsRequired().HasDefaultValue(false);

          modelBuilder.Entity<User>()
               .Property(u => u.Status)
               .HasColumnName("status")
               .HasColumnType("smallint")
               .IsRequired().HasDefaultValue(UserStatus.Active);


          modelBuilder.Entity<RefreshToken>()
               .ToTable("refreshtoken", "infra") 
               .HasKey(u => u.Id).HasName("refresh_token_pkey"); 

          modelBuilder.Entity<RefreshToken>()
               .Property(u => u.Id)
               .HasColumnName("id")
               .HasColumnType("integer")
               .IsRequired();

           modelBuilder.Entity<RefreshToken>()
               .Property(u => u.Token)
               .HasColumnName("token")
               .HasColumnType("varchar(256)")
               .IsRequired();

           modelBuilder.Entity<RefreshToken>()
               .Property(u => u.Username)
               .HasColumnName("username")
               .HasColumnType("varchar(32)")
               .IsRequired();

           modelBuilder.Entity<RefreshToken>()
               .Property(u => u.ExpiresAt)
               .HasColumnName("expiresat")
               .HasColumnType("timestampz")
               .IsRequired();

          modelBuilder.Entity<RefreshToken>()
               .Property(u => u.CreatedAt)
               .HasColumnName("createdat")
               .HasColumnType("timestampz")
               .IsRequired();

          modelBuilder.Entity<RefreshToken>()
               .Property(u => u.IsRevoked)
               .HasColumnName("isrevoked")
               .HasColumnType("boolean")
               .IsRequired();
     }

}
