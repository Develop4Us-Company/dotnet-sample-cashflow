using AppProject.Core.Infrastructure.Database.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.Financial;

public class TbFinancialCategoryConfiguration : IEntityTypeConfiguration<TbFinancialCategory>
{
    public void Configure(EntityTypeBuilder<TbFinancialCategory> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
