using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.Financial;

[Table("AccountsReceivable")]
public class TbAccountReceivable : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid FinancialCategoryId { get; set; }

    [ForeignKey(nameof(FinancialCategoryId))]
    public TbFinancialCategory FinancialCategory { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = default!;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTimeOffset DueDate { get; set; }

    [Required]
    public bool IsReceived { get; set; }

    public DateTimeOffset? ReceiptDate { get; set; }

    public Guid? FinancialAccountId { get; set; }

    [ForeignKey(nameof(FinancialAccountId))]
    public TbFinancialAccount? FinancialAccount { get; set; }
}
