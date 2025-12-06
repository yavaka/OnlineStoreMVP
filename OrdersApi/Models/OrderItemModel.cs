namespace OrdersApi.Models;

public class OrderItemModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}