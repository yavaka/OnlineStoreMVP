using PaymentsApi.Enums;

namespace PaymentsApi.Models;

public class PaymentModel
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? TxId { get; set; }
}
