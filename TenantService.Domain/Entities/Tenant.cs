using TenantService.Domain.Enums;

namespace TenantService.Domain.Entities;

// Entity che rappresenta un Tenant nel sistema
public class Tenant 
{
    public Tenant()
    {
        //Users = new List<User>();
        Id = Guid.NewGuid();
        Code = $@"tnn_{this.Id.ToString().Substring(0, 8)}";
        Name = $@"Tenant {Id}";
        TaxCode = $@"TAXCODE";
        Email = "name@domain.ext";
        SubscriptionDate = DateTime.Now;
        DisposalDate = null;
        Status = TenantStatus.Inactive;
        Notes = null;   
    }

    public Tenant(string code, string name, string taxCode, string email, TenantStatus status) : this()
    {
        Code = code;
        Name = name;
        TaxCode = taxCode;
        Email = email;
        Status = status;
    }

    public Tenant(string code, string name, string taxCode, string email, DateTime subscriptionDate, TenantStatus status) 
        : this(code, name, taxCode, email, status)
    {
        SubscriptionDate = subscriptionDate;
    }

    public Guid Id { get; set; }  //ID that identifies tenant company example: fiorencis
    public string Code { get; set; }  //string ID that identifies tenant company example: fiorencis
    public string Name { get; set; } // Tenant company name example: Fiorencis srl
    public string TaxCode { get; set; }
    public string Email { get; set; }
    public DateTime SubscriptionDate { get; set; }
    public DateTime? DisposalDate { get; set; }
    public string? Notes { get; set; } // description
    public TenantStatus Status { get; set; }  // Enum 
    
    // Logica di dominio e validazioni
    public bool IsActive => Status == TenantStatus.Active;
    
    // Users that has access to tenants data form
    //public ICollection<User> Users { get; set; }
}
