using System;
using AppProject.Models;

namespace AppProject.Core.Models.Financial;

public class FinancialCategorySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
}
