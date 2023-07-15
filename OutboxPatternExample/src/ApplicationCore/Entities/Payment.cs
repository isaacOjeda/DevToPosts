namespace ApplicationCore.Entities;

public class Payment
{
    public Guid PaymentId { get; set; }
    public string TokenCard { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; }
    public string CardHolderEmail { get; set; }
    public string CardHolderName { get; set; }
    public PaymentStatus Status { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Authorized,
    Paid,
    Rejected
}