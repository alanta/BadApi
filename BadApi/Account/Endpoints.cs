using BadApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BadApi.Account;

/// <summary>
/// Endpoints used to manage accounts.
/// </summary>
public static class Endpoints
{
    public static void Register(WebApplication app)
    {
        app.MapGet("/users", (Delegate)List)
            .RequireAuthorization();
        app.MapGet("/users/{id:int}", (Delegate)Show)
            .RequireAuthorization();
        
        var group = app.MapGroup("account")
            // best practice: Require authorization for all endpoints in this group
            .RequireAuthorization();
        
        group.MapPost("/login", (Delegate)Login);
        group.MapPost("/password", (Delegate)ResetPassword);
    }
    
    public static async Task<IResult> List([FromServices]Database db)
    {
        var model = db.Users.List();
        return Results.Ok(model);
    }
    
    public static async Task<IResult> Show(int id, [FromServices]Database db)
    {
        var model = await db.Users.FindById(id);
        return model != null ? Results.Ok(model) : Results.NotFound();
    }
    
    /// <summary>
    /// Login handled in BasicAuthenticationHandler
    /// </summary>
    public static async Task<IResult> Login(HttpContext context)
    {
        var user = context.GetUser();

        return Results.Ok(user);
    }

    /// <summary>
    /// Change my password
    /// </summary>
    public static async Task<IResult> ResetPassword([FromBody]string password, HttpContext context, [FromServices]Database db)
    {
        // Request is authenticated, user is the current user
        var user = context.GetUser();
        
        var success = db.Users.SetPassword(user.Id, password);

        return success ? Results.Ok() : Results.Conflict();
    }
}
