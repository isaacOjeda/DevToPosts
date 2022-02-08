namespace Basket.Entities;

public class Basket
{
    public int BasketId { get; set; }
    public ICollection<BasketProduct> Products { get; set; } =
        new HashSet<BasketProduct>();
}
