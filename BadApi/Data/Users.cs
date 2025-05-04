using System.Data.Common;
using Microsoft.Data.Sqlite;
using BadApi.OverPosting;
using Microsoft.AspNetCore.Identity;

namespace BadApi.Data;

public class Users(SqliteConnection db)
{
    private static readonly PasswordHasher<UserEntity> PasswordHasher = new (); // Borrowing the password hasher from ASP.NET Core Identity
    
    public async Task<UserEntity?> FindById(int id, CancellationToken cancellationToken = default)
    {
        var command = new SqliteCommand("SELECT id, name, hashedpassword, roles FROM users WHERE id = @id LIMIT 1", db);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapUserEntity(reader);
    }

    private static UserEntity? MapUserEntity(DbDataReader reader)
    {
        return new UserEntity
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            HashedPassword = reader.GetString(2),
            Roles = reader.GetString(3).Split(';')
        };
    }
    
    public async Task<UserEntity?> FindByName(string name, CancellationToken cancellationToken = default)
    {
        var command = new SqliteCommand("SELECT id, name, hashedpassword, roles FROM users WHERE name = @name LIMIT 1", db);
        command.Parameters.AddWithValue("name", name);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapUserEntity(reader);
    }
    
    public UserDetailsResponse[] List()
    {
        var command = new SqliteCommand("SELECT id, name, roles FROM users", db);
        using var reader = command.ExecuteReader();
        
        var result = new List<UserDetailsResponse>();
        while (reader.Read())
        {
            // Mapping straight into the response object to:
            // - remove the need for additional mapping code
            // - prevent exposing the password hash
            result.Add(new UserDetailsResponse(reader.GetInt32(0), reader.GetString(1), reader.GetString(2).Split(';')));
        }

        return result.ToArray();
    }
    
    public bool SetUserName(int id, string name)
    {
        var command = new SqliteCommand("UPDATE users SET name = @name WHERE id = @id", db);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("id", id);

        return command.ExecuteNonQuery() > 0;
    }
    
    public UserEntity Upsert(UserEntity entity)
    {
        // ⚠️ Upserts like these are dangerous. Prefer using targeted updates or a proper ORM
        if( entity.Id == 0)
        {
            var command =
                new SqliteCommand(
                    "INSERT INTO users(id, name, hashedpassword, roles) VALUES (NULL, @name, @hashedPassword, @roles); SELECT last_insert_rowid()",
                    db);
            
            if( string.IsNullOrWhiteSpace(entity.HashedPassword) )
            {
                // Assign a temporary password
                entity.HashedPassword = HashPassword(entity.Name + "pw");
            }
            
            command.Parameters.AddWithValue("name", entity.Name);
            command.Parameters.AddWithValue("hashedPassword", entity.HashedPassword );
            command.Parameters.AddWithValue("salt", entity.Name);
            command.Parameters.AddWithValue("roles", string.Join(';', entity.Roles ?? []));

            object? rowId = command.ExecuteScalar();
            
            entity.Id = Convert.ToInt32(rowId);
        }
        else
        {
            var command =
                new SqliteCommand(
                    "UPDATE users SET name = @name, roles = @roles WHERE id = @id",
                    db);
            
            command.Parameters.AddWithValue("name", entity.Name);
            // Not allowing updates on password and salt
            command.Parameters.AddWithValue("roles", string.Join(';', entity.Roles ?? []));
            command.Parameters.AddWithValue("id", entity.Id);

            command.ExecuteNonQuery();
        }

        return entity;
    }

    public static string HashPassword(string password)
    {
        return PasswordHasher.HashPassword(null!, password);
    }

    private static bool VerifyHashedPassword(UserEntity user, string providedPassword)
    {
        return PasswordHasher.VerifyHashedPassword(null!, user.HashedPassword, providedPassword) == PasswordVerificationResult.Success;
    }

    public async Task<UserDetailsResponse?> Login(string userName, string password)
    {
        var user = await FindByName(userName);
        if (user != null)
        {
            if (VerifyHashedPassword(user, password))
            {
                return new UserDetailsResponse(user.Id, user.Name, user.Roles);;
            }
        }

        return null;
    }

    public bool SetPassword(int userId, string password)
    {
        var hashedpassword = HashPassword(password);
        
        var command = new SqliteCommand("UPDATE users SET hashedpassword = @hashedpassword WHERE id = @id", db);
        command.Parameters.AddWithValue("hashedpassword", hashedpassword);
        command.Parameters.AddWithValue("id", userId);

        return command.ExecuteNonQuery() > 0;
    }
}