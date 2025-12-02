# Environment Prerequisites Setup

This document describes how to prepare a local development machine to run the OnlineStoreMVP solution.

## Purpose

Ensure every developer has the same baseline tools installed so they can build, run, and debug the Aspire-based microservices locally. The repository's projects target .NET 10.0, and some local container images and tooling need to match that target.

## Prerequisites

- Windows 10/11 with __Visual Studio 2022__ (or later) and the __.NET Aspire__ and __ASP.NET and web development__ workloads installed.
- Local admin rights on the machine.
- Sufficient disk space and permissions to run containers.
- Git installed and configured: verify with __git --version__.
- WSL2 enabled (recommended) for Docker Desktop integration.

## Installation Steps

### .NET 10.0 SDK

- Download and install the __.NET 10.0__ SDK from the official .NET website.
- Open a new terminal and verify: __dotnet --version__ shows a 10.x.x SDK.

Note: The repository's projects target `.NET 10.0` (net10.0). If you see a different SDK on your machine, either install the matching SDK or use a `global.json` pinned to a supported 10.x version.

### Docker Desktop

- Install Docker Desktop for Windows.
- Enable __Use WSL 2 based engine__ if prompted and ensure an active WSL2 distro (e.g., Ubuntu).
- Start Docker Desktop and confirm it reports `Running`.

**Note**: The solution's Dockerfiles are configured to use .NET 10.0 base images:
- Runtime base image: `mcr.microsoft.com/dotnet/aspnet:10.0`
- SDK image: `mcr.microsoft.com/dotnet/sdk:10.0`

The following Dockerfiles are included and ready to use:
- `CatalogApi/Dockerfile`
- `CustomersApi/Dockerfile`
- `OrdersApi/Dockerfile`
- `PaymentsApi/Dockerfile`

You can verify the Dockerfiles are correctly configured by checking that they reference `10.0` image tags. To build container images locally, use:
```bash
docker build -t catalogapi -f CatalogApi/Dockerfile .
docker build -t customersapi -f CustomersApi/Dockerfile .
docker build -t ordersapi -f OrdersApi/Dockerfile .
docker build -t paymentsapi -f PaymentsApi/Dockerfile .
```

### Dapr CLI and runtime

- Install the Dapr CLI: for example __winget install Dapr.CLI__ or follow the [Dapr documentation](https://docs.dapr.io/getting-started/install-dapr-cli/).
- Initialize the local Dapr runtime: __dapr init__
- Verify: __dapr --version__ prints both CLI and runtime versions.

**Notes:**
- The solution uses Dapr sidecars for all microservices (CatalogApi, CustomersApi, OrdersApi, PaymentsApi).
- Dapr components are located in `OnlineStoreMVP.AppHost/Configurations/dapr-components/`:
  - `pubsub.yaml` - Configures Redis for pub/sub messaging
  - `statestore.yaml` - Configures Redis for state management
- The AppHost project automatically locates these components using the `DaprHelpers.GetDaprComponentYamlPath()` method.
- When running the AppHost from Visual Studio, Dapr sidecars will be automatically started for each service with the correct component configuration.

### Redis for local development

The solution uses Redis for both pub/sub messaging and state management via Dapr components.

Run Redis container:
```bash
docker run -d --name redis -p 6379:6379 redis
```

Confirm it is running:
```bash
docker ps
```
This should show the `redis` container as `Up`.

**Note:** The Dapr components (`pubsub.yaml` and `statestore.yaml`) are configured to connect to Redis at `127.0.0.1:6379`. Ensure the Redis container is running before starting the application.

### Running the distributed application (development)

**Using Visual Studio:**
1. Ensure Redis container is running (see above).
2. Set `OnlineStoreMVP.AppHost` as the startup project.
3. Build and run the solution (F5 or Ctrl+F5).
4. The AppHost will:
   - Start all microservices (CatalogApi, CustomersApi, OrdersApi, PaymentsApi)
   - Attach Dapr sidecars to each service
   - Load Dapr components from `OnlineStoreMVP.AppHost/Configurations/dapr-components/`
   - Open the Aspire dashboard in your browser

**Using Command Line:**
1. Start Redis container: `docker run -d --name redis -p 6379:6379 redis`
2. Navigate to the solution root directory.
3. Run: `dotnet run --project OnlineStoreMVP.AppHost/OnlineStoreMVP.AppHost.csproj`

**Project Structure:**
- `OnlineStoreMVP.AppHost` - Aspire orchestration host (uses Aspire 13.0.1)
- `OnlineStoreMVP.ServiceDefaults` - Shared Aspire configuration
- `CatalogApi`, `CustomersApi`, `OrdersApi`, `PaymentsApi` - Microservice APIs
- All services use Dapr sidecars for distributed systems capabilities

## Optional local tooling

- **Postman** or an HTTP client (e.g., VS Code REST Client extension) for API testing. The solution includes `.http` files in each API project for testing.
- **Docker Compose** or `docker compose` for multi-container orchestration (if you prefer to manage containers separately).
- **Dapr Dashboard** - Access via `dapr dashboard` command to monitor sidecars and components.
- **Aspire Dashboard** - Automatically available when running the AppHost, provides observability for all services.

## Troubleshooting

**SDK Version Issues:**
- If `dotnet --version` shows a different SDK, install .NET 10.0 or add a `global.json` to pin the SDK version used for build.

**Docker Issues:**
- If Docker fails to start, ensure WSL2 is enabled and virtualization is enabled in BIOS.
- Verify Docker Desktop is running: `docker ps` should work without errors.

**Dapr Issues:**
- If `dapr init` fails, run it with admin privileges and check the Dapr logs with `dapr --version` and `dapr status`.
- If Dapr sidecars fail to start, verify the Dapr components directory exists at `OnlineStoreMVP.AppHost/Configurations/dapr-components/`.
- Check Dapr logs: `dapr logs -a <app-id>` (e.g., `dapr logs -a catalogapi`).

**Port Conflicts:**
- Services expose ports `8080` and `8081`. If these are in use, stop other services or change container port mappings.
- Redis uses port `6379`. Ensure this port is available or update the Dapr component YAML files.

**Docker Build Issues:**
- If Docker builds fail, ensure you have the .NET 10.0 SDK installed locally (required for building).
- Verify Dockerfiles reference `mcr.microsoft.com/dotnet/aspnet:10.0` and `mcr.microsoft.com/dotnet/sdk:10.0`.
- Note: Local development using `dotnet run` uses the installed SDK and doesn't require Docker.

**Redis Connection Issues:**
- Ensure Redis container is running: `docker ps | grep redis`
- Verify Redis is accessible: `docker exec -it redis redis-cli ping` should return `PONG`
- Check Dapr component YAML files point to the correct Redis host (`127.0.0.1:6379`).

## Acceptance Criteria

Before starting development, verify:

- ✅ `dotnet --version` returns a 10.x.x SDK version.
- ✅ `dapr --version` returns valid CLI and runtime versions.
- ✅ `docker ps` shows Redis container running.
- ✅ Docker Desktop is running and accessible.
- ✅ Visual Studio 2022 (or later) with .NET Aspire workload is installed.
- ✅ Dockerfiles are configured to use .NET 10.0 base images (verified in `CatalogApi/Dockerfile`, `CustomersApi/Dockerfile`, `OrdersApi/Dockerfile`, `PaymentsApi/Dockerfile`).
- ✅ AppHost is able to start service projects and Dapr sidecars (confirm via AppHost logs, Aspire dashboard, or Dapr dashboard).

**Quick Verification Commands:**
```bash
dotnet --version          # Should show 10.x.x
dapr --version            # Should show CLI and runtime versions
docker ps                 # Should show redis container
docker exec -it redis redis-cli ping  # Should return PONG
```

Developers who meet the above checks are ready to clone and run the OnlineStoreMVP solution locally.

## Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Dapr Documentation](https://docs.dapr.io/)
- [Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard) - Available at the URL shown when AppHost starts

