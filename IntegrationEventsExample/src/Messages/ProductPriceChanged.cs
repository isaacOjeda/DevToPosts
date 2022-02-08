namespace Messages;

public class ProductPriceChanged
{

    public ProductPriceChanged(int productId, double newPrice)
    {
        ProductId = productId;
        NewPrice = newPrice;
    }
    public int ProductId { get; set; }
    public double NewPrice { get; set; }
}