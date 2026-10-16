using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;

namespace AppProject.Core.Services.Financial;

public class FinancialCategorySummaryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IFinancialCategorySummaryService
{
    public async Task<SummariesResponse<FinancialCategorySummary>> GetSummariesAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialCategories, cancellationToken: cancellationToken);

        var searchText = request.SearchText?.Trim();

        var financialCategorySummaries = await databaseRepository.GetByConditionAsync<TbFinancialCategory, FinancialCategorySummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText));
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<FinancialCategorySummary>
        {
            Summaries = financialCategorySummaries
        };
    }

    public async Task<SummaryResponse<FinancialCategorySummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialCategories, cancellationToken: cancellationToken);

        var financialCategorySummary = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialCategory, FinancialCategorySummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (financialCategorySummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<FinancialCategorySummary>
        {
            Summary = financialCategorySummary
        };
    }
}
