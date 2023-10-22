using Grpc.Net.Client;
using GrpcProductServer;
using Microsoft.AspNetCore.Mvc;

namespace GrpcProductClient.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly Auth.AuthClient _grpcClient;

    public AuthController(IConfiguration configuration)
    {
        var grpcAddress = configuration["GrpcAddress"] ?? throw new InvalidOperationException("GrpcAddress not found!");
        var channel = GrpcChannel.ForAddress(grpcAddress);
        _grpcClient = new Auth.AuthClient(channel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        try
        {
            var reply = await _grpcClient.CreateAsync(request);
            return Ok(reply);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var reply = await _grpcClient.LoginAsync(request);
            return Ok(reply);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}