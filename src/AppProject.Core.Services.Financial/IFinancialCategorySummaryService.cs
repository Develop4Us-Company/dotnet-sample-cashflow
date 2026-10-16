using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IFinancialCategorySummaryService
    : ITransientService,
    IGetSummaries<SearchRequest, SummariesResponse<FinancialCategorySummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<FinancialCategorySummary>>
{
}
