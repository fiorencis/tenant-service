namespace TenantService.Application;

public class PasswordSettings
{
    int _minLength = 8;
    bool _requireUppercase = true;
    bool _requireLowercase = true;
    bool _requireDigit = true;
    bool _requireSpecialCharacter = true;

    public int MinLength
    {
        get => _minLength;
        set
        {
            if (value < 1)
                throw new ArgumentException("MinLength must be greater than 0.");

            _minLength = value;
        }
    }

    public bool RequireUppercase
    {
        get => _requireUppercase;
        set => _requireUppercase = value;
    }

    public bool RequireLowercase
    {
        get => _requireLowercase;
        set => _requireLowercase = value;
    }

    public bool RequireDigit
    {
        get => _requireDigit;
        set => _requireDigit = value;
    }

    public bool RequireSpecialCharacter
    {
        get => _requireSpecialCharacter;
        set => _requireSpecialCharacter = value;
    }
}
