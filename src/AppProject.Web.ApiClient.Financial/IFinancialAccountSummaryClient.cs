using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IFinancialAccountSummaryClient
{
    [Get("/api/financial/FinancialAccountSummary/GetSummaries")]
    public Task<SummariesResponse<FinancialAccountSummary>> GetSummariesAsync([Query] SearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/financial/FinancialAccountSummary/GetSummary")]
    public Task<SummaryResponse<FinancialAccountSummary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
