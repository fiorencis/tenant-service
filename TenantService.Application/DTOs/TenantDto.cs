namespace TenantService.Application.DTOs;

public class TenantDto
{
    public string? Id { get; set; }  // ID that identifies tenant company example: fiorencis
    public string? Code { get; set; }  // tring ID that identifies tenant company example: fiorencis
    public string? Name { get; set; } // Tenant company name example: Fiorencis srl
    public string? TaxCode { get; set; }
    public string? Email { get; set; }
    public DateTime SubscriptionDate { get; set; }
    public DateTime? DisposalDate { get; set; }
    public string? Notes { get; set; } 
    public int Status { get; set; }  
}
