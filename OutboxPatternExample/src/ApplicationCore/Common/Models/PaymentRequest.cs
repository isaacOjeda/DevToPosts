namespace ApplicationCore.Common.Models;

public record PaymentRequest(
    string TokenCard,
    double Amount,
    string Currency,
    string CardHolderEmail,
    string CardHolderName
);