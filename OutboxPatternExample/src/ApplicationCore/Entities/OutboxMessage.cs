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
    public DateTime? SentAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
}