
namespace MultiTenantSingleDatabase.Models;

/// <summary>
/// Entity Tenant
/// </summary>
public class Tenant
{
    public int TenantId { get; set; }
    public string? Name { get; set; }
    public string? Identifier { get; set; }
    public string? ConnectionString { get; set; }
}
