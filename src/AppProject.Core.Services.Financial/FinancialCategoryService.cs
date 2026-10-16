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

public class FinancialCategoryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IFinancialCategoryService
{
    public async Task<EntityResponse<FinancialCategory>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialCategories, cancellationToken: cancellationToken);

        var financialCategory = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialCategory, FinancialCategory>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (financialCategory == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<FinancialCategory>
        {
            Entity = financialCategory
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<FinancialCategory> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialCategories, cancellationToken: cancellationToken);
        await this.ValidateFinancialCategoryAsync(request.Entity, cancellationToken);

        var tbFinancialCategory = request.Entity.Adapt<TbFinancialCategory>();
        await databaseRepository.InsertAndSaveAsync(tbFinancialCategory, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbFinancialCategory.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<FinancialCategory> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialCategories, cancellationToken: cancellationToken);
        await this.ValidateFinancialCategoryAsync(request.Entity, cancellationToken);

        var tbFinancialCategory = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialCategory>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbFinancialCategory == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbFinancialCategory);

        await databaseRepository.UpdateAndSaveAsync(tbFinancialCategory, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbFinancialCategory.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageFinancialCategories, cancellationToken: cancellationToken);

        var tbFinancialCategory = await databaseRepository.GetFirstOrDefaultAsync<TbFinancialCategory>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbFinancialCategory == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        if (await databaseRepository.HasAnyAsync<TbAccountPayable>(
                query => query.Where(x => x.FinancialCategoryId == request.Id),
                cancellationToken)
            || await databaseRepository.HasAnyAsync<TbAccountReceivable>(
                query => query.Where(x => x.FinancialCategoryId == request.Id),
                cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_FinancialCategory_InUse);
        }

        await databaseRepository.DeleteAndSaveAsync(tbFinancialCategory, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateFinancialCategoryAsync(FinancialCategory financialCategory, CancellationToken cancellationToken = default)
    {
        if (await databaseRepository.HasAnyAsync<TbFinancialCategory>(
            query => query.Where(x => x.Name == financialCategory.Name && x.Id != financialCategory.Id),
            cancellationToken))
        {
            throw new AppException(ExceptionCode.Financial_FinancialCategory_DuplicateName);
        }
    }
}
