# ServiceResult

[English](#english) | [Türkçe](#türkçe)

---

<a name="english"></a>
## 🇬🇧 English

**ServiceResult** is a lightweight, zero-dependency `Result` & `Result<T>` pattern library built for both modern **ASP.NET Core Microservices / Minimal APIs (.NET 8+)** and legacy **.NET Framework / WCF Services (.NET Standard 2.0)**. It eliminates exception-driven control flow and automatically maps domain errors to **RFC 9457 `ProblemDetails`** HTTP responses in Minimal APIs.

### ✨ Features

- **Multi-Targeted Architecture (`netstandard2.0` & `net8.0`):** Use the exact same `Result<T>` contract across modern .NET 8/9/10 microservices, class libraries, and .NET Framework 4.7.2/4.8 WCF services.
- **Zero External Dependencies:** Relies solely on built-in framework APIs without bloating your dependency tree.
- **Automatic Minimal API Mapping:** Built-in `IEndpointFilter` (`.WithResultMapping()`) automatically converts `Result` and `Result<T>` return values into `200 OK`, `204 No Content`, or standardized `ProblemDetails` responses.
- **Clean Service Layer with Implicit Operators:** Return either a value (`return dto;`) or an error (`return Error.NotFound(...);`) directly without verbose wrapper calls.
- **Low-Allocation Design:** `Error` is implemented as a `readonly record struct` to minimize heap allocations.

### 📦 Installation

```bash
dotnet add package ServiceResult
```

---

### ⚙️ Multi-Target `.csproj` Configuration (Library / Package Setup)

If you are building or configuring a multi-targeted library targeting both WCF (`.NET Standard 2.0`) and Minimal APIs (`.NET 8.0+`):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net8.0</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- ASP.NET Core Minimal API & ProblemDetails support is enabled only on .NET 8+ -->
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <FrameworkReference Include="Microsoft.AspNetCore.App"/>
  </ItemGroup>

</Project>
```

---

### 🚀 Usage 1: ASP.NET Core Microservice / Minimal API (.NET 8+)

#### 1. Microservice `.csproj` Example

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ServiceResult" Version="1.0.0"/>
  </ItemGroup>

</Project>
```

#### 2. Application Service & Minimal API Endpoint (`Program.cs`)

```csharp
using ServiceResult;
using ServiceResult.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Apply automatic Result<T> -> HTTP Status / ProblemDetails mapping to the entire route group
var orders = app.MapGroup("/api/orders")
    .WithResultMapping();

orders.MapGet("/{id:int}", async (int id, IOrderService service) =>
{
    // No if/else boilerplate needed — returns 200 OK or 404 ProblemDetails automatically
    return await service.GetByIdAsync(id);
});

orders.MapPost("/", async (CreateOrderRequest request, IOrderService service) =>
{
    var result = await service.CreateAsync(request);
    // Optional explicit mapping when you want a custom 201 Created status code
    return result.ToHttpResult(StatusCodes.Status201Created);
});

app.Run();

public sealed record OrderDto(int Id, string CustomerCode, decimal TotalAmount);
public sealed record CreateOrderRequest(string CustomerCode, decimal TotalAmount);

public interface IOrderService
{
    Task<Result<OrderDto>> GetByIdAsync(int id);
    Task<Result<OrderDto>> CreateAsync(CreateOrderRequest request);
}

public sealed class OrderService : IOrderService
{
    public Task<Result<OrderDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
            return Task.FromResult<Result<OrderDto>>(
                Error.Validation("Order.InvalidId", "Order ID must be greater than zero."));

        if (id != 42)
            return Task.FromResult<Result<OrderDto>>(
                Error.NotFound("Order.NotFound", $"Order #{id} was not found."));

        // Implicit conversion from OrderDto to Result<OrderDto>
        return Task.FromResult<Result<OrderDto>>(new OrderDto(42, "CUST-100", 1250.00m));
    }

    public Task<Result<OrderDto>> CreateAsync(CreateOrderRequest request)
    {
        if (request.TotalAmount <= 0)
            return Task.FromResult<Result<OrderDto>>(
                Error.Validation("Order.InvalidAmount", "Total amount must be positive."));

        return Task.FromResult<Result<OrderDto>>(new OrderDto(43, request.CustomerCode, request.TotalAmount));
    }
}
```

#### Automatic HTTP Error Output (RFC 9457 `ProblemDetails`)

When `GetByIdAsync(99)` returns `Error.NotFound(...)`, the client automatically receives `404 Not Found`:

```json
{
  "type": "[https://tools.ietf.org/html/rfc9110#section-15.5.5](https://tools.ietf.org/html/rfc9110#section-15.5.5)",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Order #99 was not found.",
  "errorCode": "Order.NotFound"
}
```

---

### 🏢 Usage 2: WCF Service (.NET Framework 4.8 / CoreWCF)

Because `ServiceResult` targets `netstandard2.0`, legacy WCF services and shared domain contracts can use the exact same `Result<T>` and `Error` types without pulling in any ASP.NET Core dependencies.

#### 1. WCF Service `.csproj` Example

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="System.ServiceModel"/>
    <Reference Include="System.Runtime.Serialization"/>
    <PackageReference Include="ServiceResult" Version="1.0.0"/>
  </ItemGroup>

</Project>
```

#### 2. WCF Service Contract & Implementation

```csharp
using System.Runtime.Serialization;
using System.ServiceModel;
using ServiceResult;

namespace LegacyBilling.Wcf
{
    [ServiceContract]
    public interface IBillingService
    {
        [OperationContract]
        Result<InvoiceDto> GetInvoice(string invoiceNumber);
    }

    [DataContract]
    public sealed class InvoiceDto
    {
        [DataMember]
        public string InvoiceNumber { get; set; } = string.Empty;

        [DataMember]
        public decimal Amount { get; set; }
    }

    public sealed class BillingService : IBillingService
    {
        public Result<InvoiceDto> GetInvoice(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                return Error.Validation("Invoice.EmptyNumber", "Invoice number is required.");
            }

            if (invoiceNumber != "INV-2026-001")
            {
                return Error.NotFound("Invoice.NotFound", $"Invoice '{invoiceNumber}' does not exist.");
            }

            return new InvoiceDto
            {
                InvoiceNumber = invoiceNumber,
                Amount = 4890.50m
            };
        }
    }
}
```

---

### 🗂️ Error Types & HTTP Status Mapping

| `ErrorType` | Factory Method | HTTP Status Code | ProblemDetails Title |
| :--- | :--- | :--- | :--- |
| `ErrorType.Validation` | `Error.Validation(...)` | `400 Bad Request` | Validation Error |
| `ErrorType.Failure` | `Error.Failure(...)` | `400 Bad Request` | Bad Request |
| `ErrorType.Unauthorized` | `Error.Unauthorized(...)` | `401 Unauthorized` | Unauthorized |
| `ErrorType.Forbidden` | `Error.Forbidden(...)` | `403 Forbidden` | Forbidden |
| `ErrorType.NotFound` | `Error.NotFound(...)` | `404 Not Found` | Resource Not Found |
| `ErrorType.Conflict` | `Error.Conflict(...)` | `409 Conflict` | Conflict |
| `ErrorType.Unexpected` | `Error.Unexpected(...)` | `500 Internal Server Error` | Internal Server Error |

---

<a name="türkçe"></a>
## 🇹🇷 Türkçe

**ServiceResult**, hem modern **ASP.NET Core Mikroservis / Minimal API (.NET 8+)** projeleri hem de mevcut **.NET Framework / WCF Servisleri (.NET Standard 2.0)** için geliştirilmiş sıfır harici bağımlılıklı bir `Result` ve `Result<T>` yanıt sarmalayıcı kütüphanesidir. İş kuralları için `try/catch` ve exception fırlatma maliyetini ortadan kaldırır; Minimal API projelerinde domain hatalarını otomatik olarak **RFC 9457 `ProblemDetails`** HTTP yanıtlarına dönüştürür.

### ✨ Özellikler

- **Çoklu Hedef Mimarisi (`netstandard2.0` & `net8.0`):** Aynı `Result<T>` sözleşmesini hem modern .NET 8/9/10 mikroservislerinde hem de .NET Framework 4.