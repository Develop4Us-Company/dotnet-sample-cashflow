using AppProject.Core.Infrastructure.Database.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.Financial;

public class TbAccountReceivableConfiguration : IEntityTypeConfiguration<TbAccountReceivable>
{
    public void Configure(EntityTypeBuilder<TbAccountReceivable> builder)
    {
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.FinancialCategory)
            .WithMany(x => x.AccountsReceivable)
            .HasForeignKey(x => x.FinancialCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FinancialAccount)
            .WithMany(x => x.AccountsReceivable)
            .HasForeignKey(x => x.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
