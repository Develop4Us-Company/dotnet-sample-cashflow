using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IAccountReceivableClient
{
    [Get("/api/financial/AccountReceivable/Get")]
    public Task<EntityResponse<AccountReceivable>> GetAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Post("/api/financial/AccountReceivable/Post")]
    public Task<KeyResponse<Guid>> PostAsync([Body] CreateOrUpdateRequest<AccountReceivable> request, CancellationToken cancellationToken = default);

    [Put("/api/financial/AccountReceivable/Put")]
    public Task<KeyResponse<Guid>> PutAsync([Body] CreateOrUpdateRequest<AccountReceivable> request, CancellationToken cancellationToken = default);

    [Delete("/api/financial/AccountReceivable/Delete")]
    public Task<EmptyResponse> DeleteAsync([Query] DeleteRequest<Guid> request, CancellationToken cancellationToken = default);
}
