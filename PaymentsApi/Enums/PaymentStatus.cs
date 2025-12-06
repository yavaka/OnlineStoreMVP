namespace PaymentsApi.Enums;

public enum PaymentStatus
{
    Pending = 0,      // Payment initiated but not processed
    Processing = 1,   // Payment is being processed
    Completed = 2,    // Payment successful
    Failed = 3,       // Payment failed
    Refunded = 4,     // Payment was refunded
    Cancelled = 5     // Payment was cancelled
}
