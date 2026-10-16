using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IAccountPayableSummaryClient
{
    [Get("/api/financial/AccountPayableSummary/GetSummaries")]
    public Task<SummariesResponse<AccountPayableSummary>> GetSummariesAsync([Query] AccountPayableSummarySearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/financial/AccountPayableSummary/GetSummary")]
    public Task<SummaryResponse<AccountPayableSummary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
