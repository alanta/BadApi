using BadApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BadApi.OverPosting;

/// <summary>
/// Endpoints used to demonstrate over posting vulnerabilities.
/// </summary>
/// <remarks>
/// No authentication is required to keep the example simple.
/// </remarks>
public class Endpoints
{
    public static void Register(WebApplication app)
    {
        app.MapPost("/overposting/updatenamev1", (Delegate)UpdateNameV1);
        app.MapPost("/overposting/updatenamev2", (Delegate)UpdateNameV2);
        app.MapPost("/overposting/updatenamev4", (Delegate)UpdateNameV4);
    }
    
    // Very bad:
    // - Accepts the entire entity, without validation
    // - Assign any other property on the entity, like roles
    // - Upserts the entity, creating a new user if the ID is 0
    // - Returns the entire entity, including the hashed password
    public static async Task<IResult> UpdateNameV1([FromBody]UserEntity model)
    {
        DatabaseUtils.Upsert(model);
        return Results.Ok(model);
    }
    
    // Better:
    // - Validate inputs
    // - Targeted update
    // - Return a 200 response, nu further information is disclosed
    public static async Task<IResult> UpdateNameV2([FromBody]UpdateUserNameRequest model)
    {
        // Validate input: make sure the request is valid
        if( string.IsNullOrWhiteSpace(model.Name) )
        {
            return Results.ValidationProblem([new(nameof(model.Name), ["Name is required"])] );
        }
        
        // Validate input: make sure the entity exists 
        var user = await DatabaseUtils.FindById(model.Id);
        if( user == null )
        {
            return Results.NotFound();
        }
        
        // Mitigate over posting by only updating the name on the entity loaded from the database
        user.Name = model.Name;
        DatabaseUtils.Upsert(user);
        
        return Results.Ok();
    }
    
    public static async Task<IResult> UpdateNameV4([FromBody]UpdateUserNameRequest model)
    {
        // Mitigate over posting with targeted update
        var success = DatabaseUtils.SetUserName(model.Id, model.Name);
        
        return success ? Results.Accepted() : Results.BadRequest();
    }
}


