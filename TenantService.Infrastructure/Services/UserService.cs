using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TenantService.Application;
using TenantService.Application.DTOs;
using TenantService.Application.Extensions;
using TenantService.Application.Repositories;
using TenantService.Domain;
using TenantService.Domain.Entities;
using TenantService.Domain.Exceptions;
using TenantService.Domain.Security;

namespace TenantService.Infrastructure.Services;

public class UserService : ApplicationService, IUserService
{
	private readonly IRepository<User> _userRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IPasswordHasher _passwordHasher;
	private readonly DataFoldersOptions _folders;

	public UserService(IRepository<User> userRepository, IUnitOfWork unitOfWork, 
		IPasswordHasher passwordHasher, IConfiguration config, IOptions<DataFoldersOptions> folders,
		ILogger<UserService> logger) : base(config,logger)
	{
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
		_passwordHasher = passwordHasher;
		_folders = folders.Value;
	}

	public async Task<Result<Guid>> ValidateUserCredentialsAsync(string username, string password, 
		CancellationToken cancellationToken = default)
	{
		var normalizedUsername = username?.Trim();

		if (string.IsNullOrWhiteSpace(normalizedUsername))
		{
			return Result<Guid>.Failure("Username is required",  HttpStatusCode.NotFound);
			//throw new ArgumentException("Username is required.", nameof(username));
		}

		if (string.IsNullOrWhiteSpace(password))
		{
			return Result<Guid>.Failure("Password is required",  HttpStatusCode.NotFound);
			//throw new ArgumentException("Password is required.", nameof(password));
		}

		// verifies username existance
		var user = await _userRepository.ListAsync(x => x.Username == normalizedUsername, cancellationToken);

		if (user == null || user.Count == 0)
		{
			return Result<Guid>.Failure("Credenziali non corrette",  HttpStatusCode.Unauthorized);
		}

		var existingUser = user.FirstOrDefault()!;

		// and checks the password
		if(_passwordHasher.VerifyPassword(password, existingUser.PasswordHash))
		{
			return Result<Guid>.Success(existingUser.Id);
		}
		else
		{
			return Result<Guid>.Failure("Credenziali non corrette",  HttpStatusCode.Unauthorized);
		}
	}


	public async Task<Guid> AddUserAsync(UserDto user, CancellationToken cancellationToken = default)
	{
		if (user.Id == null || string.IsNullOrWhiteSpace(user.Id))
		{
			user.Id = Guid.NewGuid().ToString();
		}	

		if (!Guid.TryParse(user.Id, out var userId))
		{
			throw new ArgumentException("Invalid user id.", nameof(user));
		}

		var normalizedUsername = user.Username?.Trim();

		if (string.IsNullOrWhiteSpace(normalizedUsername))
		{
			throw new ArgumentException("Username is required.", nameof(user));
		}

		if (string.IsNullOrWhiteSpace(user.Password))
		{
			throw new ArgumentException("Password is required.", nameof(user));
		}

		if (await _userRepository.ExistsAsync(x => x.Username == normalizedUsername, cancellationToken))
		{
			throw new UsernameAlreadyExistsException(normalizedUsername);
		}

		var passwordHash = _passwordHasher.HashPassword(user.Password);
		var newUser = user.ToCreateUser(passwordHash);
		newUser.Id = userId;
		newUser.Username = normalizedUsername;
		
		await _userRepository.AddAsync(newUser, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return newUser.Id;
	}

	public async Task<Guid> UpdateUserAsync(UserDto user, CancellationToken cancellationToken = default)
	{
		var userentity = await _userRepository.GetByIdAsync(Guid.Parse(user.Id), cancellationToken);

		if (userentity == null)
		{
			throw new UserNotFoundException(user.Id);
		}

		var normalizedUsername = user.Username?.Trim();

		if (string.IsNullOrWhiteSpace(normalizedUsername))
		{
			throw new ArgumentException("Username is required.", nameof(user));
		}

		if (await _userRepository.ExistsAsync(x => x.Username == normalizedUsername && x.Id != userentity.Id, cancellationToken))
		{
			throw new UsernameAlreadyExistsException(normalizedUsername);
		}

		// If new password is provided, hash it and update the PasswordHash property
		var passwordHash = user.Password != null && user.Password.Length > 0
			? _passwordHasher.HashPassword(user.Password)
			: userentity.PasswordHash;		

		var updUser = user.ToUpdateUser(userentity, passwordHash);
		await _userRepository.UpdateAsync(updUser, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return userentity.Id;
	}

	public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

		if (user == null)
		{
			throw new UserNotFoundException(userId);
		}

		await _userRepository.DeleteAsync(user, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return true;
	}

	public async Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

		if (user == null)
		{
			throw new UserNotFoundException(userId);
		}

		return user.ToUserDto();
	}

	public async Task<UserDto> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
	{
		var normalizedUsername = username?.Trim();

		if (string.IsNullOrWhiteSpace(normalizedUsername))
		{
			throw new ArgumentException("Username is required.", nameof(username));
		}


		var user = await _userRepository.ListAsync(x => x.Username == normalizedUsername, cancellationToken);

		if (user == null || user.Count == 0)
		{
			throw new UserNotFoundException(normalizedUsername);
		}

		return user.FirstOrDefault()!.ToUserDto();
	}

	public async Task<string> GetUserImagePath(Guid userId)
    {
        return Path.Combine(
            _folders.ImageDirectory,
			"users",
            userId.ToString(),
            "avatar.webp");
    }


	public async Task<bool> VerifyUserPasswordAsync(string userId, string password)
	{
		var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));

		if (user == null)
		{
			throw new UserNotFoundException(userId);
		}

		return _passwordHasher.VerifyPassword(password, user.PasswordHash);
	}










	public async Task<string> GenerateJwtTokenAsync(Guid userId)
	{
		var user = await _userRepository.GetByIdAsync(userId);
		if (user == null)
		{
			throw new UserNotFoundException(userId);
		}

		return GenerateJwtToken(user);
	}

	private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username)
            //new Claim(ClaimTypes.Role, user.Role)
        };

        var secret = Environment.GetEnvironmentVariable("TOKEN_KEY") ?? _config.GetValue<string>("TokenKey") 
		?? "supersecretkey1234567890!@#$%^&*()wlf";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "tenantService",
            audience: "tenantClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_config.GetValue<int>("TokenValidMins", 30)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}
