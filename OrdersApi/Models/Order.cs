using OrdersApi.Enums;

namespace OrdersApi.Models;

public class Order
{
    public Guid Id { get; set; }
    public required Guid CustomerId { get; set; }
    public required DateTime OrderDateTime { get; set; }
    public required OrderStatus Status { get; set; }
    public List<OrderItemModel> Items { get; set; } = [];
}
