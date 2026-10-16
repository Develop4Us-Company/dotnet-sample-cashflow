using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IFinancialAccountClient
{
    [Get("/api/financial/FinancialAccount/Get")]
    public Task<EntityResponse<FinancialAccount>> GetAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Post("/api/financial/FinancialAccount/Post")]
    public Task<KeyResponse<Guid>> PostAsync([Body] CreateOrUpdateRequest<FinancialAccount> request, CancellationToken cancellationToken = default);

    [Put("/api/financial/FinancialAccount/Put")]
    public Task<KeyResponse<Guid>> PutAsync([Body] CreateOrUpdateRequest<FinancialAccount> request, CancellationToken cancellationToken = default);

    [Delete("/api/financial/FinancialAccount/Delete")]
    public Task<EmptyResponse> DeleteAsync([Query] DeleteRequest<Guid> request, CancellationToken cancellationToken = default);
}
