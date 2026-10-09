using AppProject.Core.Infrastructure.Database.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.Financial;

public class TbFinancialAccountConfiguration : IEntityTypeConfiguration<TbFinancialAccount>
{
    public void Configure(EntityTypeBuilder<TbFinancialAccount> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
