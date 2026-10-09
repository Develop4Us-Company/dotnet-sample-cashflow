using System;
using AppProject.Models;

namespace AppProject.Web.Models.Financial;

public class FinancialAccountSummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
}
