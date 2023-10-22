namespace GrpcProductServer.Entities;

public class UserEntity
{
    public int? Id { get; set; }
    public required string Login { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
}