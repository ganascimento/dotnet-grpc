using GrpcProductServer.Entities;
using GrpcProductServer.Security;
using Microsoft.EntityFrameworkCore;

namespace GrpcProductServer.Context;

public class AppDataContext : DbContext
{
    public required DbSet<ProductEntity> Product { get; set; }
    public required DbSet<UserEntity> User { get; set; }

    public AppDataContext(DbContextOptions<AppDataContext> options) : base(options)
    { }
}