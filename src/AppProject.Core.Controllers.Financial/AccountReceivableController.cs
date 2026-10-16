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
    public class AccountReceivableController(IAccountReceivableService accountReceivableService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await accountReceivableService.GetEntityAsync(request, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<AccountReceivable> request, CancellationToken cancellationToken)
        {
            return this.Ok(await accountReceivableService.PostEntityAsync(request, cancellationToken));
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<AccountReceivable> request, CancellationToken cancellationToken)
        {
            return this.Ok(await accountReceivableService.PutEntityAsync(request, cancellationToken));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await accountReceivableService.DeleteEntityAsync(request, cancellationToken));
        }
    }
}
