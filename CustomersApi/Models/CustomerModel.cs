namespace CustomersApi.Models;

public class CustomerModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Address { get; set; }
}
