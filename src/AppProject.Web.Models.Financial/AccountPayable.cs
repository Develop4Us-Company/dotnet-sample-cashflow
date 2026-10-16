using System;
using AppProject.Models;

namespace AppProject.Web.Models.Financial;

public class AccountPayable : ObservableModel, IEntity
{
    private Guid? id;
    private Guid financialCategoryId;
    private string description = default!;
    private decimal amount;
    private DateTimeOffset dueDate = new DateTimeOffset(DateTime.Today);
    private bool isPaid;
    private DateTimeOffset? paymentDate;
    private Guid? financialAccountId;
    private byte[]? rowVersion;

    public Guid? Id { get => this.id; set => this.Set(ref this.id, value); }

    public Guid FinancialCategoryId { get => this.financialCategoryId; set => this.Set(ref this.financialCategoryId, value); }

    public string Description { get => this.description; set => this.Set(ref this.description, value); }

    public decimal Amount { get => this.amount; set => this.Set(ref this.amount, value); }

    public DateTimeOffset DueDate { get => this.dueDate; set => this.Set(ref this.dueDate, value); }

    public bool IsPaid { get => this.isPaid; set => this.Set(ref this.isPaid, value); }

    public DateTimeOffset? PaymentDate { get => this.paymentDate; set => this.Set(ref this.paymentDate, value); }

    public Guid? FinancialAccountId { get => this.financialAccountId; set => this.Set(ref this.financialAccountId, value); }

    public byte[]? RowVersion { get => this.rowVersion; set => this.Set(ref this.rowVersion, value); }
}
