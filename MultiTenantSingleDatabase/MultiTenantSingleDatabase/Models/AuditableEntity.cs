
using System;

namespace MultiTenantSingleDatabase.Models;
public class AuditableEntity
{
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
