using System.Data.SQLite;
using BadApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BadApi.SqlInjection;

public class Endpoints
{
    public record RequestModel(string Name, string Password);
    
    public static async Task<IResult> Bad([FromBody]RequestModel model)
    {
        var command = new SQLiteCommand(
            $"SELECT * FROM users WHERE name = '{model.Name}' and pw = '{model.Password}' LIMIT 1",
            DatabaseUtils.Db);

        await using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                return Results.Text($"User {reader["name"] as string} logged in.");
            }
        }

        return Results.Unauthorized();
    }

    public static async Task<IResult> Good([FromBody]RequestModel model)
    {
        
        var command = new SQLiteCommand(
            "SELECT * FROM users WHERE name = @name and pw = @pw LIMIT 1", 
            DatabaseUtils.Db);
        command.Parameters.AddWithValue("@name", model.Name);
        command.Parameters.AddWithValue("@pw", model.Password);

        await using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                return Results.Text($"User {reader["name"] as string} logged in.");
            }
        }
        
        return Results.Unauthorized();
    }
}