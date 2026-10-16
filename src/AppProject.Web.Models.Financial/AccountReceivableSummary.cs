using System;
using AppProject.Models;

namespace AppProject.Web.Models.Financial;

public class AccountReceivableSummary : ISummary
{
    public Guid Id { get; set; }

    public string FinancialCategoryName { get; set; } = default!;

    public string Description { get; set; } = default!;

    public decimal Amount { get; set; }

    public DateTimeOffset DueDate { get; set; }

    public bool IsReceived { get; set; }

    public DateTimeOffset? ReceiptDate { get; set; }

    public string? FinancialAccountName { get; set; }
}
