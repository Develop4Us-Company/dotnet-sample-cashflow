using System;
using AppProject.Models;

namespace AppProject.Web.Models.Financial;

public class AccountReceivable : ObservableModel, IEntity
{
    private Guid? id;
    private Guid financialCategoryId;
    private string description = default!;
    private decimal amount;
    private DateTimeOffset dueDate = new DateTimeOffset(DateTime.Today);
    private bool isReceived;
    private DateTimeOffset? receiptDate;
    private Guid? financialAccountId;
    private byte[]? rowVersion;

    public Guid? Id { get => this.id; set => this.Set(ref this.id, value); }

    public Guid FinancialCategoryId { get => this.financialCategoryId; set => this.Set(ref this.financialCategoryId, value); }

    public string Description { get => this.description; set => this.Set(ref this.description, value); }

    public decimal Amount { get => this.amount; set => this.Set(ref this.amount, value); }

    public DateTimeOffset DueDate { get => this.dueDate; set => this.Set(ref this.dueDate, value); }

    public bool IsReceived { get => this.isReceived; set => this.Set(ref this.isReceived, value); }

    public DateTimeOffset? ReceiptDate { get => this.receiptDate; set => this.Set(ref this.receiptDate, value); }

    public Guid? FinancialAccountId { get => this.financialAccountId; set => this.Set(ref this.financialAccountId, value); }

    public byte[]? RowVersion { get => this.rowVersion; set => this.Set(ref this.rowVersion, value); }
}
