namespace MediatrValidationExample.Domain;
public class Product
{
    public int ProductId { get; set; }
    public string Description { get; set; } = default!;
    public double Price { get; set; }
}
