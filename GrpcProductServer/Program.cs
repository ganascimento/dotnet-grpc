using GrpcProductServer.Context;
using GrpcProductServer.Security;
using GrpcProductServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseInMemoryDatabase("testdb"));

builder.Services.AddScoped<JwtFactory>();
builder.Services.ConfigureAuthorization(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ProductService>();
app.MapGrpcService<AuthService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
