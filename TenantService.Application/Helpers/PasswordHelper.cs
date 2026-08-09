namespace TenantService.Application;

public static class PasswordHelper
{
    public static bool ValidatePassword(string password, PasswordSettings options, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrEmpty(password))
        {
            errorMessage = "Password cannot be empty.";
            return false;
        }

        // Check for minimum length
        if (password.Length < options.MinLength)
        {
            errorMessage = $"Password must be at least {options.MinLength} characters long.";
            return false;
        }

        // Check for at least one uppercase letter
        if (options.RequireUppercase && !password.Any(char.IsUpper))
        {
            errorMessage = "Password must contain at least one uppercase letter.";
            return false;
        }

        // Check for at least one lowercase letter
        if (options.RequireLowercase && !password.Any(char.IsLower))
        {
            errorMessage = "Password must contain at least one lowercase letter.";
            return false;
        }

        // Check for at least one digit
        if (options.RequireDigit && !password.Any(char.IsDigit))
        {
            errorMessage = "Password must contain at least one digit.";
            return false;
        }

        // Check for at least one special character
        if (options.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errorMessage = "Password must contain at least one special character.";
            return false;
        }

        return true;
    }
}
