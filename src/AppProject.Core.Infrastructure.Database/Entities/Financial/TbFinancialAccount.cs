using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.Financial;

[Table("FinancialAccounts")]
public class TbFinancialAccount : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    public ICollection<TbAccountPayable> AccountsPayable { get; set; } = new List<TbAccountPayable>();

    public ICollection<TbAccountReceivable> AccountsReceivable { get; set; } = new List<TbAccountReceivable>();
}
