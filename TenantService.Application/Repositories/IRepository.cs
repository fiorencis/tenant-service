using TenantService.Domain.Entities;

namespace TenantService.Application.Repositories;

public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T>
    where T : class
{
    
}