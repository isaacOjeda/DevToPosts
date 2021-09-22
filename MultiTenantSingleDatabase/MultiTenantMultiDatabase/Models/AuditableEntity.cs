namespace MultiTenantSingleDatabase.Models;
public class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
