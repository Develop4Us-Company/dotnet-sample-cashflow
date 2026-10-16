using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IAccountPayableClient
{
    [Get("/api/financial/AccountPayable/Get")]
    public Task<EntityResponse<AccountPayable>> GetAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Post("/api/financial/AccountPayable/Post")]
    public Task<KeyResponse<Guid>> PostAsync([Body] CreateOrUpdateRequest<AccountPayable> request, CancellationToken cancellationToken = default);

    [Put("/api/financial/AccountPayable/Put")]
    public Task<KeyResponse<Guid>> PutAsync([Body] CreateOrUpdateRequest<AccountPayable> request, CancellationToken cancellationToken = default);

    [Delete("/api/financial/AccountPayable/Delete")]
    public Task<EmptyResponse> DeleteAsync([Query] DeleteRequest<Guid> request, CancellationToken cancellationToken = default);
}
