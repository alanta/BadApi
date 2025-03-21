using System.ComponentModel.DataAnnotations;

namespace BadApi.OverPosting;

public class UserEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string HashedPassword { get; set; }
    public string[] Roles { get; set; } = [];
}

public record UserDetailsResponse(int Id, string Name, string[] Roles);

public record UpdateUserNameRequest(int Id, string Name);