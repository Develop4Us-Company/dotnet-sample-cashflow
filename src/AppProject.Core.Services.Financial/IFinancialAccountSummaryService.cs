using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IFinancialAccountSummaryService
    : ITransientService,
    IGetSummaries<SearchRequest, SummariesResponse<FinancialAccountSummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<FinancialAccountSummary>>
{
}
