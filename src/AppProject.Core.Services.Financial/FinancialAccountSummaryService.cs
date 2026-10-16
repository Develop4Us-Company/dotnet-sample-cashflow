using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;

namespace AppProject.Core.Services.Financial;

public class FinancialAccountSummaryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IFinancialAccountSummaryService
{
    public async Task<SummariesResponse<FinancialAccountSummary>> GetSummariesAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialAccounts, cancellationToken: cancellationToken);

        var searchText = request.SearchText?.Trim();

        var financialAccountSummaries = await databaseRepository.GetByConditionAsync<TbFinancialAccount, FinancialAccountSummary>(
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

        return new SummariesResponse<FinancialAccountSummary>
        {
            Summaries = financialAccountSummaries
        };
    }

    public async Task<SummaryResponse<FinancialAccountSummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialAccounts, cancellationToken: cancellationToken);

        var financialAccountSummary = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialAccount, FinancialAccountSummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (financialAccountSummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<FinancialAccountSummary>
        {
            Summary = financialAccountSummary
        };
    }
}
