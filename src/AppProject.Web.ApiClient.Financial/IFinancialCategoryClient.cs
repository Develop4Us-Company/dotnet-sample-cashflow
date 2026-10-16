using System;
using AppProject.Models;
using AppProject.Web.Models.Financial;
using Refit;

namespace AppProject.Web.ApiClient.Financial;

public interface IFinancialCategoryClient
{
    [Get("/api/financial/FinancialCategory/Get")]
    public Task<EntityResponse<FinancialCategory>> GetAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Post("/api/financial/FinancialCategory/Post")]
    public Task<KeyResponse<Guid>> PostAsync([Body] CreateOrUpdateRequest<FinancialCategory> request, CancellationToken cancellationToken = default);

    [Put("/api/financial/FinancialCategory/Put")]
    public Task<KeyResponse<Guid>> PutAsync([Body] CreateOrUpdateRequest<FinancialCategory> request, CancellationToken cancellationToken = default);

    [Delete("/api/financial/FinancialCategory/Delete")]
    public Task<EmptyResponse> DeleteAsync([Query] DeleteRequest<Guid> request, CancellationToken cancellationToken = default);
}
