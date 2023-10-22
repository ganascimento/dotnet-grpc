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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var reply = await _grpcClient.GetAllAsync(new GetAllProductRequest(), GetHeader());
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

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProductRequest request)
    {
        try
        {
            var reply = await _grpcClient.UpdateAsync(request, GetHeader());
            return Ok(reply);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(DeleteProductRequest request)
    {
        try
        {
            var reply = await _grpcClient.DeleteAsync(request, GetHeader());
            return Ok(reply);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}