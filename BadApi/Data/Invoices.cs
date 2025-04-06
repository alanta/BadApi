using System.Data.SQLite;

namespace BadApi.Data;

public class Invoices(SQLiteConnection db)
{
    public InvoiceDetailsResponse[] List(int? userId = null)
    {
        var command =
            new SQLiteCommand(
                "SELECT invoices.id, users.name, invoice_number, amount_payable, currency, due_date, description, status FROM invoices INNER JOIN users ON users.id=user_id",
                db);

        if (userId != null)
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

    public InvoiceDetailsResponse? FindByNumber(string invoiceNumber, int? userId = null)
    {
        var command =
            new SQLiteCommand(
                "SELECT invoices.id, users.name, invoice_number, amount_payable, currency, due_date, description, status FROM invoices INNER JOIN users ON users.id=user_id WHERE invoice_number = @invoice_number LIMIT 1",
                db);
        command.Parameters.AddWithValue("invoice_number", invoiceNumber);

        if (userId != null)
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