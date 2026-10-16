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
    public class AccountReceivableSummaryController(IAccountReceivableSummaryService accountReceivableSummaryService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSummariesAsync([FromQuery] AccountReceivableSummarySearchRequest request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await accountReceivableSummaryService.GetSummariesAsync(request, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await accountReceivableSummaryService.GetSummaryAsync(request, cancellationToken));
        }
    }
}
