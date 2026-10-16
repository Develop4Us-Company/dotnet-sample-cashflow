using AppProject.Core.Services.Financial;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.Financial
{
    [Route("api/financial/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FinancialAccountBalanceSummaryController(IFinancialAccountBalanceSummaryService financialAccountBalanceSummaryService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSummariesAsync([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await financialAccountBalanceSummaryService.GetSummariesAsync(request, cancellationToken));
        }
    }
}
