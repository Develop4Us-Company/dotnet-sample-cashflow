using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.Financial;

public class FinancialCategory : IEntity
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    public byte[]? RowVersion { get; set; }
}
