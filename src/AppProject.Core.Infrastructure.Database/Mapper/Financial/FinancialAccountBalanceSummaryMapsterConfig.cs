using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper.Financial;

public class FinancialAccountBalanceSummaryMapsterConfig : IRegisterMapsterConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TbFinancialAccount, FinancialAccountBalanceSummary>()
            .Map(dest => dest.Balance, src =>
                (src.AccountsReceivable.Where(x => x.IsReceived).Sum(x => (decimal?)x.Amount) ?? 0m)
                - (src.AccountsPayable.Where(x => x.IsPaid).Sum(x => (decimal?)x.Amount) ?? 0m));
    }
}
