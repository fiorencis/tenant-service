namespace TenantService.Domain.Entities;

public class DbUpdate
{
    public int Id { get; set; }
    public DateTime AppliedAt { get; set; }
    public string Version {get; set;}
}