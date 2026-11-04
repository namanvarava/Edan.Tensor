# EDAN Tensor

**Universal Data Tensor Platform**

EDAN Tensor is a .NET-based universal data platform designed around human convenience, modular architecture, and extensible data integration.

## Initial repository foundation

This repository starts with a .NET 10 modular-monolith foundation. The initial solution separates Platform, Commercial, Licensing, and Host responsibilities while keeping module dependencies explicit.

### Architecture principles

- Domain projects contain business rules and remain independent of infrastructure and UI frameworks.
- Application projects contain use cases and depend on their own Domain layer.
- Infrastructure projects implement persistence and external-system adapters.
- Contracts contain transport or integration DTOs, not Domain entities.
- Commercial and Licensing communicate through versioned integration contracts and asynchronous messaging boundaries; neither module accesses the other's persistence implementation.
- API and Worker hosts are composition roots.
- No production signing keys, credentials, or secrets are stored in this repository.

### Initial technology baseline

- .NET 10 / C# 14
- ASP.NET Core for API hosts
- SQLite is the planned initial platform/commercial/licensing metadata store; database adapters are added in later implementation steps.
- DuckDB and Parquet remain planned data-engine components and are not provisioned by this foundation commit.

### Build

Install the .NET 10 SDK, then run:

```powershell
dotnet --info
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore
```

The initial foundation intentionally avoids third-party NuGet dependencies. Provider SDKs, database packages, API documentation packages, and test frameworks will be added in dedicated steps after compatibility and version validation.

### Initial hosts

- `EDAN.Tensor.ApiHost`: product and commercial API composition root.
- `EDAN.Tensor.LicenseAuthority.Host`: isolated licensing authority host boundary.
- `EDAN.Tensor.WorkerHost`: background processing host foundation.

### Development status

This is an architectural foundation, not a completed product implementation. Business workflows, database migrations, authentication, key management, payment integrations, email delivery, WPF applications, and automated test frameworks are added incrementally and must be validated before production use.
