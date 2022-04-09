namespace MediatrExample.ApplicationCore.Domain;
public class Product : BaseEntity
{
    public int ProductId { get; set; }
    public string Description { get; set; } = default!;
    public double Price { get; set; }
}
