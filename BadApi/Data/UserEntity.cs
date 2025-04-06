namespace BadApi.Data;

public class UserEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? HashedPassword { get; set; }
    public string[] Roles { get; set; } = [];
}