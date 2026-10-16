using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IFinancialAccountBalanceSummaryService
    : ITransientService,
    IGetSummaries<SearchRequest, SummariesResponse<FinancialAccountBalanceSummary>>
{
}
