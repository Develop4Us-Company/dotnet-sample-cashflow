using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IAccountPayableService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<AccountPayable>>,
    IPostEntity<CreateOrUpdateRequest<AccountPayable>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<AccountPayable>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
