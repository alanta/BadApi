using BadApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BadApi.OverPosting;

/// <summary>
/// Endpoints used to demonstrate over-posting vulnerabilities, leading to privilege escalation
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
        app.MapPost("/overposting/updatenamev3", (Delegate)UpdateNameV3);
    }
    
    // Very bad:
    // - Accepts the entire entity, without validation
    // - Assign any other property on the entity, like roles
    // - Will update other users as well
    // - Upserts the entity, creating a new user if the ID is 0
    public static async Task<IResult> UpdateNameV1([FromBody]UserEntity model, [FromServices]Database db)
    {
        db.Users.Upsert(model);
        return Results.Ok(model);
    }
    
    // Better:
    // - Validate inputs
    // - Targeted update
    // - Returns a 200 response, no further information is disclosed
    public static async Task<IResult> UpdateNameV2([FromBody]UpdateUserNameRequest model, [FromServices]Database db)
    {
        // Validate input: make sure the request is valid
        if( string.IsNullOrWhiteSpace(model.Name) )
        {
            return Results.ValidationProblem( 
                new Dictionary<string, string[]>
                {
                    { nameof(model.Name), new[] { "Name is required." } }
                });
        }
        
        // Validate input: make sure the entity exists 
        var user = await db.Users.FindById(model.Id);
        if( user == null )
        {
            return Results.NotFound();
        }
        
        // Mitigate over-posting by only updating the name on the entity loaded from the database
        user.Name = model.Name;
        db.Users.Upsert(user);
        
        return Results.Ok(); // Don't return excess information
    }
    
    public static async Task<IResult> UpdateNameV3([FromBody]UpdateUserNameRequest model, [FromServices]Database db)
    {
        // Mitigate over posting with targeted update
        var success = db.Users.SetUserName(model.Id, model.Name);
        
        return success ? Results.Accepted() : Results.BadRequest();
    }
}


