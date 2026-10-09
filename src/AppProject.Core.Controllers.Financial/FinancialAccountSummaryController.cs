using AppProject.Core.Services.Financial;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.Financial
{
    [Route("api/financial/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FinancialAccountSummaryController(IFinancialAccountSummaryService financialAccountSummaryService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSummariesAsync([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await financialAccountSummaryService.GetSummariesAsync(request, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await financialAccountSummaryService.GetSummaryAsync(request, cancellationToken));
        }
    }
}
