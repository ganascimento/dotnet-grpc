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

    public override async Task<ProductListReply> GetAll(GetAllProductRequest request, ServerCallContext context)
    {
        var result = await _context.Product.ToListAsync();
        var productListReply = new ProductListReply();

        if (result == null) return productListReply;
        productListReply.Products.AddRange(result.Select(item => this.Parser(item)));

        return productListReply;
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

    public override async Task<ProductReply> Update(UpdateProductRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Description) || request.Value <= 0 || request.Id <= 0)
            return new ProductReply();

        var product = await _context.Product.FirstOrDefaultAsync(item => item.Id == request.Id);
        if (product == null) return new ProductReply();

        product.Name = request.Name;
        product.Description = request.Description;
        product.Value = request.Value;

        _context.Product.Update(product);
        await _context.SaveChangesAsync();

        return this.Parser(product);
    }

    public override async Task<ProductReply> Delete(DeleteProductRequest request, ServerCallContext context)
    {
        if (request.Id <= 0) return new ProductReply();

        var product = await _context.Product.FirstOrDefaultAsync(item => item.Id == request.Id);
        if (product == null) return new ProductReply();

        _context.Product.Remove(product);
        await _context.SaveChangesAsync();

        return this.Parser(product);
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