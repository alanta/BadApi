using Microsoft.Data.Sqlite;
using BadApi.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BadApi.SqlInjection;

/// <summary>
/// Sql injection.
/// </summary>
public static class Endpoints
{
    public static void Setup(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
    }
    public static void Register(WebApplication app)
    {
        var group = app.MapGroup("sqlinjection")
            .AllowAnonymous();
        group.MapPost("/bad", (Delegate)Bad);
        group.MapPost("/better", (Delegate)Better);
        group.MapPost("/good", (Delegate)Good);
    }

    public static async Task<IResult> Bad([FromBody]LoginRequest model, [FromServices]Database db)
    {
        // Don't store plaintext passwords in the database, obviously
        var hashedPassword = Users.HashPassword(model.Password);
        
        var command = new SqliteCommand(
            $"SELECT * FROM users WHERE name = '{model.Name}' and hashedpassword = '{hashedPassword}'",
            db.Db);

        await using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                return Results.Text($"User {reader["name"] as string} logged in.");
            }
        }

        return Results.Unauthorized();
    }
    
    public static async Task<IResult> Better([FromBody]LoginRequest model, [FromServices]Database db)
    {
        // We don't store plaintext passwords in the database, obviously
        var hashedPassword = Users.HashPassword(model.Password);
        
        var command = new SqliteCommand(
            // Use parameterized queries to prevent SQL injection
            "SELECT * FROM users WHERE name = @name and hashedpassword = @pw", 
            db.Db);
            
        command.Parameters.AddWithValue("@name", model.Name);
        command.Parameters.AddWithValue("@pw", hashedPassword);

        await using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                return Results.Text($"User {reader["name"] as string} logged in.");
            }
        }

        return Results.Unauthorized();
    }
    
    #region Improved
    public static async Task<IResult> Good(
        [FromBody]LoginRequest model, 
        [FromServices]Database db,
        [FromServices]IValidator<LoginRequest> validator,
        [FromServices]ILogger<Program> logger)
    {
        // Validate the model using FluentValidation
        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            // Return 400 Bad Request with validation errors
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        try
        {
            var command = new SqliteCommand(
                // Use parameterized queries to prevent SQL injection
                // Limit the columns and number of records returned to 1 for both performance and security
                "SELECT 1 FROM users WHERE name = @name and hashedpassword = @pw LIMIT 1", 
                db.Db);
            
            command.Parameters.AddWithValue("@name", model.Name);
            command.Parameters.AddWithValue("@pw", Users.HashPassword(model.Password));

            await using var reader = command.ExecuteReader();
            if (reader.Read()) // expecting only 1 record, so no need to loop
            {
                logger.LogInformation("User {Name} logged in.", model.Name);
                return Results.Text($"User {reader["name"] as string} logged in.");
            }
        }
        catch (Exception e)
        {
            // Always log errors for security related problems, but don't disclose sensitive information
            // * Don't log the password or personal information
            // * Don't return an error status, always return 401 Unauthorized
            logger.LogError(e, "Failed to process login request for user {Name}", model.Name);
        }
        
        logger.LogWarning("Failed login attempt for user {Name}.", model.Name);
        return Results.Unauthorized();
    }
    
    #endregion
}