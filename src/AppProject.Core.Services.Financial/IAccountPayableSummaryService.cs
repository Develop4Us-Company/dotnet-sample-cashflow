using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IAccountPayableSummaryService
    : ITransientService,
    IGetSummaries<AccountPayableSummarySearchRequest, SummariesResponse<AccountPayableSummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<AccountPayableSummary>>
{
}
