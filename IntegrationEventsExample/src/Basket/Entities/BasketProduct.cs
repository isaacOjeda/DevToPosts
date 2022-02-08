namespace Basket.Entities;

public class BasketProduct
{
    public int BasketProductId { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public string ProductDescription { get; set; } = default!;
    public double UnitCost { get; set; }
    public double TotalCost { get; set; }

    public Basket Basket { get; set; } = default!;
}