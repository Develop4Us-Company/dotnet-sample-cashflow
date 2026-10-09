using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IFinancialAccountService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<FinancialAccount>>,
    IPostEntity<CreateOrUpdateRequest<FinancialAccount>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<FinancialAccount>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
