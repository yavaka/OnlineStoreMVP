using CommunityToolkit.Aspire.Hosting.Dapr;
using System.Collections.Immutable;
using static OnlineStoreMVP.AppHost.Helpers.DaprHelpers;

var builder = DistributedApplication.CreateBuilder(args);

var stateStore = builder.AddDaprStateStore("statestore");
var pubSub = builder.AddDaprPubSub("pubsub");
var daprComponentsPath = GetDaprComponentYamlPath();

var catalog = builder.AddProject<Projects.CatalogApi>("catalogapi")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "catalogapi",
        ResourcesPaths = daprComponentsPath,
    })
    .WithHttpHealthCheck("/health");

var customers = builder.AddProject<Projects.CustomersApi>("customersapi")
    .WithReference(catalog)
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "customersapi",
        ResourcesPaths = daprComponentsPath,
    })
    .WithHttpHealthCheck("/health");

var orders = builder.AddProject<Projects.OrdersApi>("ordersapi")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "ordersapi",
        ResourcesPaths = daprComponentsPath,
    })
    .WithHttpHealthCheck("/health");

var payments = builder.AddProject<Projects.PaymentsApi>("paymentsapi")
    .WithReference(orders)
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "paymentsapi",
        ResourcesPaths = daprComponentsPath,
    })
    .WithHttpHealthCheck("/health");

builder.Build().Run();