using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IAccountReceivableSummaryClient
{
    [Get("/api/financial/AccountReceivableSummary/GetSummaries")]
    public Task<SummariesResponse<AccountReceivableSummary>> GetSummariesAsync([Query] AccountReceivableSummarySearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/financial/AccountReceivableSummary/GetSummary")]
    public Task<SummaryResponse<AccountReceivableSummary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
