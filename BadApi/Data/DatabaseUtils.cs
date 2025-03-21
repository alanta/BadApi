using System.Data.Common;
using System.Data.SQLite;
using BadApi.OverPosting;
using Microsoft.AspNetCore.Identity;

namespace BadApi.Data;

/// <summary>
/// Provides test data for some of the endpoints.
/// Do NOT use this in production code.
/// </summary>
public static class DatabaseUtils
{
    private static readonly PasswordHasher<UserEntity> PasswordHasher = new (); // Borrowing the password hasher from ASP.NET Core Identity
    private static readonly string DbFilePath = Path.Combine(Path.GetTempPath(), "dummy.db");

    public static SQLiteConnection? Db { get; private set; }

    public static string GetConnectionString()
    {
        return "DataSource=" +DbFilePath + ";" ;
    }

    public static void Seed()
    {
        if (File.Exists(DbFilePath))
        {
            File.Delete(DbFilePath);
        }
        
        Console.WriteLine("Seeding database in " + DbFilePath);

        SQLiteConnection.CreateFile(DbFilePath);
            
        Db = new SQLiteConnection(GetConnectionString());
        Db.Open();
        // ⚠️ This is NOT how to store users and passwords. Please use the built-in ASP.NET Core Identity system or an external identity provider
        new SQLiteCommand("create table users(id INTEGER PRIMARY KEY AUTOINCREMENT, name nvarchar(20), hashedpassword nvarchar(100), roles nvarchar(100))", Db).ExecuteNonQuery();
        
        Upsert(new UserEntity { Name = "alice", HashedPassword = HashPassword("alicepw"), Roles = ["developer"] });
        Upsert(new UserEntity { Name = "bob", HashedPassword = HashPassword("bobpw"), Roles = ["developer"] });
        Upsert(new UserEntity { Name = "claire", HashedPassword = HashPassword("admin"), Roles = ["admin"] });
        
        new SQLiteCommand("create table invoices(id INTEGER PRIMARY KEY AUTOINCREMENT, user_id INTEGER, invoice_number TEXT, amount_payable REAL, currency TEXT, due_date TEXT, description TEXT, status TEXT, FOREIGN KEY(user_id) REFERENCES users(id))", Db).ExecuteNonQuery();
        
        Upsert(new UserEntity { Name = "Hiroshi", HashedPassword = HashPassword("hiroshipw"), Roles = ["purchase"] });
        Upsert(new UserEntity { Name = "Fatima", HashedPassword = HashPassword("fatimapw"), Roles = ["purchase"] });
        Upsert(new UserEntity { Name = "Lars", HashedPassword = HashPassword("larspw"), Roles = ["purchase"] });
        Upsert(new UserEntity { Name = "Priya", HashedPassword = HashPassword("priyapw"), Roles = ["purchase"] });

        new SQLiteCommand(
                @"INSERT INTO invoices(user_id, invoice_number, amount_payable, currency, due_date, description, status) VALUES
(4, 'INV001', 500, 'USD', '2025-04-01', 'Monthly subscription for office stationery', 'Open'),
(5, 'INV002', 600, 'USD', '2025-04-05', 'Printer ink', 'Paid'),
(6, 'INV003', 700, 'USD', '2025-04-10', 'Annual subscription for office cleaning supplies', 'Disputed'),
(7, 'INV004', 800, 'USD', '2025-04-15', 'Coffee and snacks', 'Open'),
(4, 'INV005', 900, 'USD', '2025-04-20', 'Quarterly subscription for ergonomic chairs', 'Paid'),
(5, 'INV006', 1000, 'USD', '2025-04-25', 'IT support services', 'Disputed'),
(6, 'INV007', 1100, 'USD', '2025-05-01', 'Monthly subscription for office plants maintenance', 'Open'),
(7, 'INV008', 1200, 'USD', '2025-05-05', 'Water dispenser service', 'Paid'),
(4, 'INV009', 1300, 'USD', '2025-05-10', 'Annual subscription for office security services', 'Disputed'),
(5, 'INV010', 1400, 'USD', '2025-05-15', 'Monthly subscription for office software licenses', 'Open'),
(6, 'INV011', 1500, 'USD', '2025-05-20', 'Office furniture rental', 'Paid'),
(7, 'INV012', 1600, 'USD', '2025-05-25', 'Annual subscription for office internet services', 'Disputed'),
(4, 'INV013', 1700, 'USD', '2025-06-01', 'Monthly subscription for office supplies delivery', 'Open'),
(5, 'INV014', 1800, 'USD', '2025-06-05', 'Printer maintenance', 'Paid'),
(6, 'INV015', 1900, 'USD', '2025-06-10', 'Annual subscription for office training programs', 'Disputed'),
(7, 'INV016', 2000, 'USD', '2025-06-15', 'Insurance', 'Open'),
(4, 'INV017', 2100, 'USD', '2025-06-20', 'Quarterly subscription for office kitchen supplies', 'Paid'),
(5, 'INV018', 2200, 'USD', '2025-06-25', 'Team building event planning services', 'Disputed'),
(6, 'INV019', 2300, 'USD', '2025-06-28', 'Monthly subscription for office recycling services', 'Open'),
(7, 'INV020', 2400, 'USD', '2025-06-30', 'Health and safety audits', 'Paid');", Db)
            .ExecuteNonQuery();
    }
 
    public static async Task<UserEntity?> FindById(int id, CancellationToken cancellationToken = default)
    {
        var command = new SQLiteCommand("SELECT id, name, hashedpassword, roles FROM users WHERE id = @id LIMIT 1", Db);
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
    
    public static async Task<UserEntity?> FindByName(string name, CancellationToken cancellationToken = default)
    {
        var command = new SQLiteCommand("SELECT id, name, hashedpassword, roles FROM users WHERE name = @name LIMIT 1", Db);
        command.Parameters.AddWithValue("name", name);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapUserEntity(reader);
    }
    
    public static UserDetailsResponse[] List()
    {
        var command = new SQLiteCommand("SELECT id, name, roles FROM users", Db);
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
    
    public static bool SetUserName(int id, string name)
    {
        var command = new SQLiteCommand("UPDATE users SET name = @name WHERE id = @id", Db);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("id", id);

        return command.ExecuteNonQuery() > 0;
    }
    
    public static UserEntity Upsert(UserEntity entity)
    {
        // ⚠️ Upserts like these are dangerous. Prefer using targeted updates or a proper ORM
        if( entity.Id == 0)
        {
            var command =
                new SQLiteCommand(
                    "INSERT INTO users(id, name, hashedpassword, roles) VALUES (NULL, @name, @hashedPassword, @roles); SELECT last_insert_rowid()",
                    Db);
            
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
                new SQLiteCommand(
                    "UPDATE users SET name = @name, roles = @roles WHERE id = @id",
                    Db);
            
            command.Parameters.AddWithValue("name", entity.Name);
            // Not allowing updates on password and salt
            command.Parameters.AddWithValue("roles", string.Join(';', entity.Roles ?? []));
            command.Parameters.AddWithValue("id", entity.Id);

            command.ExecuteNonQuery();
        }

        return entity;
    }

    private static string HashPassword(string password)
    {
        return PasswordHasher.HashPassword(null!, password);
    }

    private static bool VerifyHashedPassword(UserEntity user, string providedPassword)
    {
        return PasswordHasher.VerifyHashedPassword(null!, user.HashedPassword, providedPassword) == PasswordVerificationResult.Success;
    }

    public static async Task<UserDetailsResponse?> Login(string userName, string password)
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

    public static bool SetPassword(int userId, string password)
    {
        var hashedpassword = HashPassword(password);
        
        var command = new SQLiteCommand("UPDATE users SET hashedpassword = @hashedpassword WHERE id = @id", Db);
        command.Parameters.AddWithValue("hashedpassword", hashedpassword);
        command.Parameters.AddWithValue("id", userId);

        return command.ExecuteNonQuery() > 0;
    }

    public static class Invoices
    {
        public static InvoiceDetailsResponse[] List(int? userId = null)
        {
            var command = new SQLiteCommand("SELECT invoices.id, users.name, invoice_number, amount_payable, currency, due_date, description, status FROM invoices INNER JOIN users ON users.id=user_id", Db);

            if( userId != null )
            {
                command.CommandText += " WHERE user_id = @userId";
                command.Parameters.AddWithValue("userId", userId);
            }
            
            command.CommandText += " ORDER BY due_date ASC";
            
            var result = new List<InvoiceDetailsResponse>();
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new InvoiceDetailsResponse
                (
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDecimal(3),
                    reader.GetString(4),
                    reader.GetDateTime(5),
                    reader.GetString(6),
                    reader.GetString(7)
                ));
            }
            
            return result.ToArray();
        }
        
        public static InvoiceDetailsResponse? FindByNumber(string invoiceNumber, int? userId = null)
        {
            var command = new SQLiteCommand("SELECT invoices.id, users.name, invoice_number, amount_payable, currency, due_date, description, status FROM invoices INNER JOIN users ON users.id=user_id WHERE invoice_number = @invoice_number LIMIT 1", Db);
            command.Parameters.AddWithValue("invoice_number", invoiceNumber);
            
            if( userId != null )
            {
                command.CommandText += " AND user_id = @userId";
                command.Parameters.AddWithValue("userId", userId);
            }

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new InvoiceDetailsResponse
            (
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDecimal(3),
                reader.GetString(4),
                reader.GetDateTime(5),
                reader.GetString(6),
                reader.GetString(7)
            );
        }
    }
}

public record InvoiceDetailsResponse(string User, string InvoiceNumber, decimal AmountPayable, string Currency, DateTime DueDate, string Description, string Status);