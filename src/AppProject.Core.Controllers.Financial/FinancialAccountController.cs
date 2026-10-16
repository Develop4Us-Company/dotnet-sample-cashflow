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
    public class FinancialAccountController(IFinancialAccountService financialAccountService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialAccountService.GetEntityAsync(request, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<FinancialAccount> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialAccountService.PostEntityAsync(request, cancellationToken));
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<FinancialAccount> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialAccountService.PutEntityAsync(request, cancellationToken));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await financialAccountService.DeleteEntityAsync(request, cancellationToken));
        }
    }
}
