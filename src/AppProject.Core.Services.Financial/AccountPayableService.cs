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

public class AccountPayableService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IAccountPayableService
{
    public async Task<EntityResponse<AccountPayable>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsPayable, cancellationToken: cancellationToken);

        var entity = await databaseRepository.GetFirstOrDefaultAsync<TbAccountPayable, AccountPayable>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (entity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<AccountPayable> { Entity = entity };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<AccountPayable> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsPayable, cancellationToken: cancellationToken);
        await this.ValidateAccountPayableAsync(request.Entity, cancellationToken);

        var entity = request.Entity.Adapt<TbAccountPayable>();
        await databaseRepository.InsertAndSaveAsync(entity, cancellationToken);

        return new KeyResponse<Guid> { Id = entity.Id };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<AccountPayable> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsPayable, cancellationToken: cancellationToken);

        var entity = await databaseRepository.GetFirstOrDefaultAsync<TbAccountPayable>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (entity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await this.ValidateAccountPayableAsync(request.Entity, cancellationToken);
        request.Entity.Adapt(entity);
        await databaseRepository.UpdateAndSaveAsync(entity, cancellationToken);

        return new KeyResponse<Guid> { Id = entity.Id };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsPayable, cancellationToken: cancellationToken);

        var entity = await databaseRepository.GetFirstOrDefaultAsync<TbAccountPayable>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (entity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(entity, cancellationToken);
        return new EmptyResponse();
    }

    private async Task ValidateAccountPayableAsync(AccountPayable entity, CancellationToken cancellationToken)
    {
        if (entity.IsPaid)
        {
            if (!entity.FinancialAccountId.HasValue || entity.FinancialAccountId.Value == Guid.Empty)
            {
                throw new AppException(ExceptionCode.Financial_AccountPayable_FinancialAccountRequired);
            }

            if (!entity.PaymentDate.HasValue)
            {
                throw new AppException(ExceptionCode.Financial_AccountPayable_PaymentDateRequired);
            }
        }

        if (!await databaseRepository.HasAnyAsync<TbFinancialCategory>(
                query => query.Where(x => x.Id == entity.FinancialCategoryId),
                cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_AccountPayable_FinancialCategoryNotFound);
        }

        if (entity.FinancialAccountId.HasValue
            && !await databaseRepository.HasAnyAsync<TbFinancialAccount>(
                query => query.Where(x => x.Id == entity.FinancialAccountId.Value),
                cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_AccountPayable_FinancialAccountNotFound);
        }
    }
}
