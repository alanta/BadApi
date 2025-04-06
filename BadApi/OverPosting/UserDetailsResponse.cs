using System.ComponentModel.DataAnnotations;
using BadApi.Data;

namespace BadApi.OverPosting;

public record UserDetailsResponse(int Id, string Name, string[] Roles)
{
    public static UserDetailsResponse Map(UserEntity entity)
    {
        return new UserDetailsResponse(entity.Id, entity.Name, entity.Roles);
    }
}