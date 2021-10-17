namespace MinimalApis.Api.Entities;
public class Product
{
    public Product(string description, double price)
    {
        Description = description;
        Price = price;
    }

    public int ProductId { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
}
