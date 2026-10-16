using System;
using AppProject.Models;

namespace AppProject.Web.Models.Financial;

public class AccountReceivableSummarySearchRequest : SearchRequest
{
    public Guid? FinancialCategoryId { get; set; }

    public DateTimeOffset? DueDateFrom { get; set; }

    public DateTimeOffset? DueDateTo { get; set; }

    public bool? IsReceived { get; set; }

    public Guid? FinancialAccountId { get; set; }
}
