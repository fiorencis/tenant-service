using TenantService.Application.DTOs;
using TenantService.Application.Extensions;

namespace TenantService.Application;

public interface IUserService : IApplicationService
{
	// Validates user credentials (username/password) and returns the result with userId Guid value
	// if username and password are correct otherwise Guid.Empty
	Task<Result<Guid>> ValidateUserCredentialsAsync(string username, string password, 
		CancellationToken cancellationToken = default);


	Task<Guid> AddUserAsync(UserDto user, CancellationToken cancellationToken = default);

	Task<Guid> UpdateUserAsync(UserDto user, CancellationToken cancellationToken = default);

	Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);

	Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

	Task<UserDto> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
	
	Task<string> GetUserImagePath(Guid userId);

}
