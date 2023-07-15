namespace ApplicationCore.Entities;

public class OutboxMessage
{
    public OutboxMessage()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string EventType { get; set; }
    public string EventBody { get; set; }
    public bool Finished { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
    public int Retries { get; set; }
    public OutboxMessageType Type { get; set; }
}

public enum OutboxMessageType
{
    PaymentCreated
}