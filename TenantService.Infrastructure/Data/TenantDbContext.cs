using System.Security.Principal;
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
          // ===========================================
          //  DbUpdate entity configuration
          // ===========================================
          modelBuilder.Entity<DbUpdate>()
               .ToTable("dbupdate", "infra") 
               .HasKey(u => u.Id).HasName("dbupdate_pkey");

          modelBuilder.Entity<DbUpdate>()
               .Property(u => u.Id)
               .HasColumnName("id")
               .HasColumnType("integer")
               .IsRequired();

          modelBuilder.Entity<DbUpdate>()
               .Property(u => u.Version)
               .HasColumnName("version")
               .HasColumnType("varchar(32)")
               .IsRequired();    

          modelBuilder.Entity<DbUpdate>()
               .Property(u => u.AppliedAt)
               .HasColumnName("appliedat")
               .HasColumnType("timestampz")
               .IsRequired()
               .HasDefaultValueSql("CURRENT_TIMESTAMP");      

          modelBuilder.Entity<DbUpdate>()
               .HasIndex(u => u.Version)
               .IsUnique();

          // ===========================================
          //  User entity configuration
          // ===========================================
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


          // ===========================================
          //  RefreshToken entity configuration
          // ===========================================
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

          
          // ===========================================
          //  Tenant entity configuration
          // ===========================================
          modelBuilder.Entity<Tenant>()
               .ToTable("tenant", "application") 
               .HasKey(u => u.Id).HasName("tenant_pkey"); 
               
          modelBuilder.Entity<Tenant>() 
               .Property(u => u.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .IsRequired();
          
          modelBuilder.Entity<Tenant>()
               .Property(u => u.Code)
               .HasColumnName("code")
               .HasColumnType("varchar(24)")
               .IsRequired();

          modelBuilder.Entity<Tenant>()
               .Property(u => u.Name)
               .HasColumnName("name")
               .HasColumnType("varchar(256)")
               .IsRequired();

          modelBuilder.Entity<Tenant>()
               .Property(u => u.TaxCode)
               .HasColumnName("taxcode")
               .HasColumnType("varchar(16)")
               .IsRequired();

          modelBuilder.Entity<Tenant>()
               .Property(u => u.Email)
               .HasColumnName("email")
               .HasColumnType("varchar(64)")
               .IsRequired();

          modelBuilder.Entity<Tenant>()
               .Property(u => u.SubscriptionDate)
               .HasColumnName("subscriptiondate")
               .HasColumnType("date")
               .IsRequired();

          modelBuilder.Entity<Tenant>()
               .Property(u => u.DisposalDate)
               .HasColumnName("disposaldate")
               .HasColumnType("date");

          modelBuilder.Entity<Tenant>()
               .Property(u => u.Status)
               .HasColumnName("status")
               .HasColumnType("smallint")
               .IsRequired();
          
          modelBuilder.Entity<Tenant>()
               .Property(u => u.Notes)
               .HasColumnName("notes")
               .HasColumnType("varchar(max)");

     }

}
