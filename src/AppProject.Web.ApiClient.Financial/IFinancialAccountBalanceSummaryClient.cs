using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IFinancialAccountBalanceSummaryClient
{
    [Get("/api/financial/FinancialAccountBalanceSummary/GetSummaries")]
    public Task<SummariesResponse<FinancialAccountBalanceSummary>> GetSummariesAsync([Query] SearchRequest request, CancellationToken cancellationToken = default);
}
