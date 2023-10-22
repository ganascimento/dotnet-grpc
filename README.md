# Dotnet gRPC

This project was developed to test gRPC implementation using dotnet with authentication

## Resources used

- DotNet 7
- gRPC
- EF Core InMemory Db
- Jwt

## What is gRPC?

gRPC, which stands for Google Remote Procedure Call, is an open-source, high-performance, and language-agnostic remote procedure call (RPC) framework. It was developed by Google but is now maintained by the Cloud Native Computing Foundation (CNCF). gRPC enables efficient communication between distributed systems, making it a popular choice for building microservices architectures and other networked applications.

Key features of gRPC include its use of the Protocol Buffers (Protobuf) data serialization format for efficient data interchange, support for multiple programming languages, and built-in features like authentication, load balancing, and error handling. gRPC uses HTTP/2 as its transport protocol, offering benefits such as multiplexing and header compression, which improve performance and reduce latency.

<p align="start">
  <img src="./assets/grpc.png" width="160" />
</p>

## Test

To run this project you need docker installed on your machine, see the docker documentation [here](https://www.docker.com/).

Having all the resources installed, run the command in a terminal from the root folder of the project and wait some seconds to build project image and download the resources: `docker-compose up -d`

In terminal show this:

```console
 [+] Running 3/3
 ✔ Network dotnet-grpc_grpc_network  Created                               0.9s
 ✔ Container server_app              Started                               2.0s
 ✔ Container client_app              Started                               2.0s
```

After this, access the link below:

- Swagger project [click here](http://localhost:5000/swagger)

### Stop Application

To stop, run: `docker-compose down`

### Request Example

`/Auth/Login`

```json
{
  "login": "admin",
  "password": "admin123"
}
```

`/Auth/Create`

```json
{
  "login": "admin",
  "password": "admin123",
  "userName": "Admin"
}
```

# How to implement

To implement gRPC you need two projects, a server project and a client project

## Server

To create server project use: `dotnet new grpc --name <name>`

Configure your `*.photo` file, if it is necessary to create a new file, it is necessary to configure:

- `*.csproj` add new line with `<Protobuf Include="Protos\<name>.proto" GrpcServices="Server" />`.
- `Program.cs` add config `app.MapGrpcService<<your_service>>();`.

After this, run `dotnet build` to construct the resources and create your service.

### Proto example

```proto
syntax = "proto3";

option csharp_namespace = "GrpcProductServer";

package greet;

service Product {
  rpc Get (GetProductRequest) returns (ProductReply);
  rpc Create (CreateProductRequest) returns (ProductReply);
}

message GetProductRequest {
  int32 id = 1;
}

message ProductReply {
  int32 id = 1;
  string name = 2;
  string description = 3;
  double value = 4;
}

message CreateProductRequest {
  string name = 1;
  string description = 2;
  double value = 3;
}
```

### Service implementation example

Use the override to implement methods, all DTO's were created when build was run

```c#
using Grpc.Core;
using GrpcProductServer.Context;
using GrpcProductServer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrpcProductServer.Services;

[Authorize]
public class ProductService : Product.ProductBase
{
    private readonly AppDataContext _context;

    public ProductService(AppDataContext context)
    {
        _context = context;
    }

    public override async Task<ProductReply> Get(GetProductRequest request, ServerCallContext context)
    {
        if (request.Id == 0) return new ProductReply();

        var result = await _context.Product.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (result == null) return new ProductReply();

        return this.Parser(result);
    }

    public override async Task<ProductReply> Create(CreateProductRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Description) || request.Value <= 0)
            return new ProductReply();

        var result = _context.Product.Add(new ProductEntity
        {
            Description = request.Description,
            Name = request.Name,
            Value = request.Value
        });

        await _context.SaveChangesAsync();

        return this.Parser(result.Entity);
    }

    private ProductReply Parser(ProductEntity entity)
    {
        return new ProductReply
        {
            Id = entity.Id ?? 0,
            Description = entity.Description,
            Name = entity.Name,
            Value = entity.Value
        };
    }
}
```

## Client

Create a project, and add packages:

- [Google.Protobuf](https://www.nuget.org/packages/Google.Protobuf/3.24.3)
- [Grpc.Net.Client](https://www.nuget.org/packages/Grpc.Net.Client/2.57.0)
- [Grpc.Tools](https://www.nuget.org/packages/Grpc.Tools/2.58.0)

Create a new folder `Protos` and add the same `*.proto` from the server project.

Into `*.csproj` configure the client protos:

```csproj
<ItemGroup>
    <Protobuf Include="Protos\product.proto" GrpcServices="Client" />
</ItemGroup>
```

After this run `dotnet build` to create resources.

### Implement client service

The method `GetHeader` is used to send the token to gRPC.

```c#
using Grpc.Core;
using Grpc.Net.Client;
using GrpcProductServer;
using Microsoft.AspNetCore.Mvc;

namespace GrpcProductClient.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly Product.ProductClient _grpcClient;

    public ProductController(IConfiguration configuration)
    {
        var grpcAddress = configuration["GrpcAddress"] ?? throw new InvalidOperationException("GrpcAddress not found!");
        var channel = GrpcChannel.ForAddress(grpcAddress);
        _grpcClient = new Product.ProductClient(channel);
    }

    private Metadata GetHeader()
    {
        var headers = new Metadata();
        var token = Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(token))
            headers.Add("Authorization", $"Bearer {token}");

        return headers;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var reply = await _grpcClient.GetAsync(new GetProductRequest { Id = id }, GetHeader());

            return Ok(reply);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        try
        {
            var reply = await _grpcClient.CreateAsync(request, GetHeader());
            return Ok(reply);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```
