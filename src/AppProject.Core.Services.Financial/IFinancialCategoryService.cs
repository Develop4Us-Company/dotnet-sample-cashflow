using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IFinancialCategoryService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<FinancialCategory>>,
    IPostEntity<CreateOrUpdateRequest<FinancialCategory>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<FinancialCategory>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
