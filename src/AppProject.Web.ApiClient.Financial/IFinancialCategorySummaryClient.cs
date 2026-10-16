using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IFinancialCategorySummaryClient
{
    [Get("/api/financial/FinancialCategorySummary/GetSummaries")]
    public Task<SummariesResponse<FinancialCategorySummary>> GetSummariesAsync([Query] SearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/financial/FinancialCategorySummary/GetSummary")]
    public Task<SummaryResponse<FinancialCategorySummary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
