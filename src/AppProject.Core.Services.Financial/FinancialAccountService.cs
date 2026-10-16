using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Mapster;

namespace AppProject.Core.Services.Financial;

public class FinancialAccountService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IFinancialAccountService
{
    public async Task<EntityResponse<FinancialAccount>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialAccounts, cancellationToken: cancellationToken);

        var financialAccount = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialAccount, FinancialAccount>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (financialAccount == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<FinancialAccount>
        {
            Entity = financialAccount
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<FinancialAccount> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialAccounts, cancellationToken: cancellationToken);
        await this.ValidateFinancialAccountAsync(request.Entity, cancellationToken);

        var tbFinancialAccount = request.Entity.Adapt<TbFinancialAccount>();
        await databaseRepository.InsertAndSaveAsync(tbFinancialAccount, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbFinancialAccount.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<FinancialAccount> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialAccounts, cancellationToken: cancellationToken);
        await this.ValidateFinancialAccountAsync(request.Entity, cancellationToken);

        var tbFinancialAccount = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialAccount>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbFinancialAccount == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbFinancialAccount);

        await databaseRepository.UpdateAndSaveAsync(tbFinancialAccount, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbFinancialAccount.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialAccounts, cancellationToken: cancellationToken);

        var tbFinancialAccount = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialAccount>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbFinancialAccount == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        if (await databaseRepository.HasAnyAsync<TbAccountPayable>(
                query => query.Where(x => x.FinancialAccountId == request.Id),
                cancellationToken)
            || await databaseRepository.HasAnyAsync<TbAccountReceivable>(
                query => query.Where(x => x.FinancialAccountId == request.Id),
                cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_FinancialAccount_InUse);
        }

        await databaseRepository.DeleteAndSaveAsync(tbFinancialAccount, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateFinancialAccountAsync(FinancialAccount financialAccount, CancellationToken cancellationToken = default)
    {
        if (await databaseRepository.HasAnyAsync<TbFinancialAccount>(
            query => query.Where(x => x.Name == financialAccount.Name && x.Id != financialAccount.Id),
            cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_FinancialAccount_DuplicateName);
        }
    }
}
