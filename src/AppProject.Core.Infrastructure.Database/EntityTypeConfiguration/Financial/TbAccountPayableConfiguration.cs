using AppProject.Core.Infrastructure.Database.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.Financial;

public class TbAccountPayableConfiguration : IEntityTypeConfiguration<TbAccountPayable>
{
    public void Configure(EntityTypeBuilder<TbAccountPayable> builder)
    {
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.FinancialCategory)
            .WithMany(x => x.AccountsPayable)
            .HasForeignKey(x => x.FinancialCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FinancialAccount)
            .WithMany(x => x.AccountsPayable)
            .HasForeignKey(x => x.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
