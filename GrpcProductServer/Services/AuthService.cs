using Grpc.Core;
using GrpcProductServer.Context;
using GrpcProductServer.Entities;
using GrpcProductServer.Security;
using Microsoft.EntityFrameworkCore;

namespace GrpcProductServer.Services;

public class AuthService : Auth.AuthBase
{
    private readonly AppDataContext _context;
    private readonly JwtFactory _jwtFactory;

    public AuthService(AppDataContext context, JwtFactory jwtFactory)
    {
        _context = context;
        _jwtFactory = jwtFactory;
    }

    public override async Task<CreateUserReply> Create(CreateUserRequest request, ServerCallContext context)
    {
        var result = _context.User.Add(new UserEntity
        {
            Login = request.Login,
            Password = PasswordHash.HashPassword(request.Password),
            UserName = request.UserName
        });

        await _context.SaveChangesAsync();

        return new CreateUserReply
        {
            Id = result.Entity.Id ?? 0,
            Login = result.Entity.Login
        };
    }

    public override async Task<TokenReply> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _context.User.FirstOrDefaultAsync(x => x.Login == request.Login);
        if (user == null)
            throw new InvalidOperationException("User or password is incorrect!");

        var isAuthenticated = PasswordHash.VerifyHashedPassword(user.Password, request.Password);

        if (isAuthenticated)
        {
            var token = _jwtFactory.GenerateJwtToken(user);

            return new TokenReply
            {
                Token = token,
                UserName = user.UserName
            };
        }
        else
            throw new InvalidOperationException("User or password is incorrect!");
    }
}