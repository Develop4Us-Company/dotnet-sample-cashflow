using System;
using AppProject.Models;

namespace AppProject.Core.Models.Financial;

public class FinancialAccountBalanceSummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public decimal Balance { get; set; }
}
