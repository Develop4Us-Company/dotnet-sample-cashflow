using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper.Financial;

public class AccountPayableSummaryMapsterConfig : IRegisterMapsterConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TbAccountPayable, AccountPayableSummary>()
            .Map(dest => dest.FinancialCategoryName, src => src.FinancialCategory.Name)
            .Map(dest => dest.FinancialAccountName, src => src.FinancialAccount == null ? null : src.FinancialAccount.Name);
    }
}
