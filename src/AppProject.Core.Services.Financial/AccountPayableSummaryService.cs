using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;

namespace AppProject.Core.Services.Financial;

public class AccountPayableSummaryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IAccountPayableSummaryService
{
    public async Task<SummariesResponse<AccountPayableSummary>> GetSummariesAsync(AccountPayableSummarySearchRequest request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsPayable, cancellationToken: cancellationToken);

        if (request.DueDateFrom.HasValue && request.DueDateTo.HasValue && request.DueDateFrom.Value > request.DueDateTo.Value)
        {
            throw new AppException(ExceptionCode.Financial_AccountPayable_InvalidDueDateRange);
        }

        var searchText = request.SearchText?.Trim();
        var summaries = await databaseRepository.GetByConditionAsync<TbAccountPayable, AccountPayableSummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x => x.Description.Contains(searchText));
                }

                if (request.FinancialCategoryId.HasValue)
                {
                    query = query.Where(x => x.FinancialCategoryId == request.FinancialCategoryId.Value);
                }

                if (request.DueDateFrom.HasValue)
                {
                    query = query.Where(x => x.DueDate >= request.DueDateFrom.Value);
                }

                if (request.DueDateTo.HasValue)
                {
                    query = query.Where(x => x.DueDate <= request.DueDateTo.Value);
                }

                if (request.IsPaid.HasValue)
                {
                    query = query.Where(x => x.IsPaid == request.IsPaid.Value);
                }

                if (request.FinancialAccountId.HasValue)
                {
                    query = query.Where(x => x.FinancialAccountId == request.FinancialAccountId.Value);
                }

                query = query.OrderBy(x => x.DueDate).ThenBy(x => x.Description).ThenBy(x => x.Id);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<AccountPayableSummary> { Summaries = summaries };
    }

    public async Task<SummaryResponse<AccountPayableSummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsPayable, cancellationToken: cancellationToken);

        var summary = await databaseRepository.GetFirstOrDefaultAsync<TbAccountPayable, AccountPayableSummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (summary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<AccountPayableSummary> { Summary = summary };
    }
}
