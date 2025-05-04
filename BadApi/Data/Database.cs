using Microsoft.Data.Sqlite;
using BadApi.OverPosting;

namespace BadApi.Data;

/// <summary>
/// Provides test data for some of the endpoints.
/// Do NOT use this in production code.
/// </summary>
public class Database
{
    private readonly string _dbFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    public SqliteConnection? Db { get; private set; }

    public string GetConnectionString()
    {
        return "DataSource=" +_dbFilePath + ";" ;
    }

    public void Seed()
    {
        if (File.Exists(_dbFilePath))
        {
            File.Delete(_dbFilePath);
        }
        
        Console.WriteLine("Seeding database in " + _dbFilePath);

        //SqliteConnection.CreateFile(_dbFilePath);
        Db = new SqliteConnection(GetConnectionString());
        
        Users = new Users(Db);
        Invoices = new Invoices(Db);
        
        Db.Open();
        // ⚠️ This is NOT how to store users and passwords. Please use the built-in ASP.NET Core Identity system or an external identity provider
        new SqliteCommand("create table users(id INTEGER PRIMARY KEY AUTOINCREMENT, name nvarchar(20), hashedpassword nvarchar(100), roles nvarchar(100))", Db).ExecuteNonQuery();
        
        Users.Upsert(new UserEntity { Name = "alice", HashedPassword = Users.HashPassword("alicepw"), Roles = ["developer"] });
        Users.Upsert(new UserEntity { Name = "bob", HashedPassword = Users.HashPassword("bobpw"), Roles = ["developer"] });
        Users.Upsert(new UserEntity { Name = "claire", HashedPassword = Users.HashPassword("admin"), Roles = ["admin"] });
        
        new SqliteCommand("create table invoices(id INTEGER PRIMARY KEY AUTOINCREMENT, user_id INTEGER, invoice_number TEXT, amount_payable REAL, currency TEXT, due_date TEXT, description TEXT, status TEXT, FOREIGN KEY(user_id) REFERENCES users(id))", Db).ExecuteNonQuery();
        
        Users.Upsert(new UserEntity { Name = "Hiroshi", HashedPassword = Users.HashPassword("hiroshipw"), Roles = ["purchase"] });
        Users.Upsert(new UserEntity { Name = "Fatima", HashedPassword = Users.HashPassword("fatimapw"), Roles = ["purchase"] });
        Users.Upsert(new UserEntity { Name = "Lars", HashedPassword = Users.HashPassword("larspw"), Roles = ["purchase"] });
        Users.Upsert(new UserEntity { Name = "Priya", HashedPassword = Users.HashPassword("priyapw"), Roles = ["purchase"] });

        new SqliteCommand(
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

    public Users Users { get; private set; }

    public Invoices Invoices {get; private set; }
}