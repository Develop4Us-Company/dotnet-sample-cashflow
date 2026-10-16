using System;
using AppProject.Core.Models.Financial;
using AppProject.Models;

namespace AppProject.Core.Services.Financial;

public interface IAccountReceivableService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<AccountReceivable>>,
    IPostEntity<CreateOrUpdateRequest<AccountReceivable>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<AccountReceivable>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
