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

public class AccountReceivableService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IAccountReceivableService
{
    public async Task<EntityResponse<AccountReceivable>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsReceivable, cancellationToken: cancellationToken);

        var entity = await databaseRepository.GetFirstOrDefaultAsync<TbAccountReceivable, AccountReceivable>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (entity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<AccountReceivable> { Entity = entity };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<AccountReceivable> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsReceivable, cancellationToken: cancellationToken);
        await this.ValidateAccountReceivableAsync(request.Entity, cancellationToken);

        var entity = request.Entity.Adapt<TbAccountReceivable>();
        await databaseRepository.InsertAndSaveAsync(entity, cancellationToken);

        return new KeyResponse<Guid> { Id = entity.Id };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<AccountReceivable> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsReceivable, cancellationToken: cancellationToken);

        var entity = await databaseRepository.GetFirstOrDefaultAsync<TbAccountReceivable>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (entity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await this.ValidateAccountReceivableAsync(request.Entity, cancellationToken);
        request.Entity.Adapt(entity);
        await databaseRepository.UpdateAndSaveAsync(entity, cancellationToken);

        return new KeyResponse<Guid> { Id = entity.Id };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsReceivable, cancellationToken: cancellationToken);

        var entity = await databaseRepository.GetFirstOrDefaultAsync<TbAccountReceivable>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (entity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(entity, cancellationToken);
        return new EmptyResponse();
    }

    private async Task ValidateAccountReceivableAsync(AccountReceivable entity, CancellationToken cancellationToken)
    {
        if (entity.IsReceived)
        {
            if (!entity.FinancialAccountId.HasValue || entity.FinancialAccountId.Value == Guid.Empty)
            {
                throw new AppException(ExceptionCode.Financial_AccountReceivable_FinancialAccountRequired);
            }

            if (!entity.ReceiptDate.HasValue)
            {
                throw new AppException(ExceptionCode.Financial_AccountReceivable_ReceiptDateRequired);
            }
        }

        if (!await databaseRepository.HasAnyAsync<TbFinancialCategory>(
                query => query.Where(x => x.Id == entity.FinancialCategoryId),
                cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_AccountReceivable_FinancialCategoryNotFound);
        }

        if (entity.FinancialAccountId.HasValue
            && !await databaseRepository.HasAnyAsync<TbFinancialAccount>(
                query => query.Where(x => x.Id == entity.FinancialAccountId.Value),
                cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_AccountReceivable_FinancialAccountNotFound);
        }
    }
}
