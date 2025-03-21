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
        var group = app.MapGroup("account")
            // best practice: Require authorization for all endpoints in this group
            .RequireAuthorization();

        group.MapPost("/account/login", (Delegate)Login);
        group.MapPost("/account/password", (Delegate)ResetPassword);
        
        app.MapGet("/users", (Delegate)List)
            .RequireAuthorization();
    }
    
    public static async Task<IResult> List()
    {
        var model = DatabaseUtils.List();
        return Results.Ok(model);
    }
    
    /// <summary>
    /// Login
    /// </summary>
    public static async Task<IResult> Login(HttpContext context)
    {
        var user = context.GetUser();

        return Results.Ok(user);
    }

    /// <summary>
    /// Change my password
    /// </summary>
    public static async Task<IResult> ResetPassword([FromBody]string password, HttpContext context)
    {
        // Request is authenticated, user is the current user
        var user = context.GetUser();
        
        var success = DatabaseUtils.SetPassword(user.Id, password);

        return success ? Results.Ok() : Results.Conflict();
    }
}
