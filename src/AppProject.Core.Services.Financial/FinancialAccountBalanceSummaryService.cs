using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Models;
using AppProject.Models.Auth;

namespace AppProject.Core.Services.Financial;

public class FinancialAccountBalanceSummaryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IFinancialAccountBalanceSummaryService
{
    public async Task<SummariesResponse<FinancialAccountBalanceSummary>> GetSummariesAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ViewFinancialAccountBalances, cancellationToken: cancellationToken);

        var searchText = request.SearchText?.Trim();
        var summaries = await databaseRepository.GetByConditionAsync<TbFinancialAccount, FinancialAccountBalanceSummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x => x.Name.Contains(searchText));
                }

                query = query.OrderBy(x => x.Name).ThenBy(x => x.Id);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<FinancialAccountBalanceSummary> { Summaries = summaries };
    }
}
