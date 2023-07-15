namespace ApplicationCore.Common.Events;

public record PaymentCreatedEvent(
    Guid PaymentId,
    string TokenCard,
    double Amount,
    string Currency,
    string CardHolderEmail,
    string CardHolderName);