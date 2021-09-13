
namespace MultiTenantSingleDatabase.Models;

public class Product : AuditableEntity
{
    public int ProductId { get; set; }
    public string Description { get; set; }
}
