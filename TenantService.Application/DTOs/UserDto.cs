namespace TenantService.Application.DTOs;

public class UserDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; } = null;
    public string? FullName { get; set; } = null;
    public int Status { get; set; } = 0;
    public bool Admin { get; set; } = false;
    public string? Password { get; set; } = null;
    public string? PasswordHash { get; set; } = null;

    public override string ToString()
    {
        return $"UserDto(Id: {Id}, Username: {Username}, FullName: {FullName}, Email: {Email}, Status: {Status}, Admin: {Admin})";
    }
}
