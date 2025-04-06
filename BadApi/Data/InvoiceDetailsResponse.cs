namespace BadApi.Data;

public record InvoiceDetailsResponse(string User, string InvoiceNumber, decimal AmountPayable, string Currency, DateTime DueDate, string Description, string Status);