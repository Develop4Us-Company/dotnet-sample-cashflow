using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;
using AppProject.Models.CustomValidators;

namespace AppProject.Core.Models.Financial;

public class AccountReceivable : IEntity
{
    public Guid? Id { get; set; }

    [RequiredGuid]
    public Guid FinancialCategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = default!;

    [Required]
    [Range(typeof(decimal), "0", "79228162514264337593543950335", MinimumIsExclusive = true)]
    public decimal Amount { get; set; }

    [Required]
    public DateTimeOffset DueDate { get; set; }

    public bool IsReceived { get; set; }

    public DateTimeOffset? ReceiptDate { get; set; }

    public Guid? FinancialAccountId { get; set; }

    public byte[]? RowVersion { get; set; }
}
