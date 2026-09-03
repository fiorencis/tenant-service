namespace TenantService.Application.DTOs;

public class UserDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public int Status { get; set; }
    
    public bool Admin { get; set; } = false;

    public string? Password { get; set; }
    public string? PasswordHash { get; set; }

    public override string ToString()
    {
        return $"UserDto(Id: {Id}, Username: {Username}, FullName: {FullName}, Email: {Email}, Status: {Status}, Admin: {Admin})";
    }
}
