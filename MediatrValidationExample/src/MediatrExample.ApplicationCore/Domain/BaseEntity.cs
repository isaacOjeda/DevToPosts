namespace MediatrExample.ApplicationCore.Domain;
public class BaseEntity
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedByAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
