using Microsoft.EntityFrameworkCore;

namespace TenantService.Infrastructure;

public class InfraDbContext(DbContextOptions<InfraDbContext> options) : DbContext(options)
{
   
}
