using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IAccountReceivableSummaryService
    : ITransientService,
    IGetSummaries<AccountReceivableSummarySearchRequest, SummariesResponse<AccountReceivableSummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<AccountReceivableSummary>>
{
}
