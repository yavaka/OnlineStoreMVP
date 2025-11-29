var builder = DistributedApplication.CreateBuilder(args);

var catalog = builder.AddProject<Projects.CatalogApi>("catalogapi");
var orders = builder.AddProject<Projects.OrdersApi>("ordersapi").WithReference(catalog);
var customers = builder.AddProject<Projects.CustomersApi>("customersapi");
var payments = builder.AddProject<Projects.PaymentsApi>("paymentsapi").WithReference(orders);

builder.Build().Run();
