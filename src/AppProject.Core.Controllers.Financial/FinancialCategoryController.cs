using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Financial;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.Financial
{
    [Route("api/financial/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FinancialCategoryController(IFinancialCategoryService financialCategoryService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialCategoryService.GetEntityAsync(request, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<FinancialCategory> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialCategoryService.PostEntityAsync(request, cancellationToken));
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<FinancialCategory> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialCategoryService.PutEntityAsync(request, cancellationToken));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialCategoryService.DeleteEntityAsync(request, cancellationToken));
        }
    }
}
