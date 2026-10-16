using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Infrastructure.Database.Mapper.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Core.Services.Financial;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Mapster;
using Moq;
using Shouldly;

namespace AppProject.Core.Tests.Financial.Services;

[TestFixture]
public class FinancialAccountBalanceSummaryServiceTests
{
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private FinancialAccountBalanceSummaryService service = null!;
    private TypeAdapterConfig mapperConfig = null!;
    private List<TbFinancialAccount> dataset = null!;

    [SetUp]
    public void SetUp()
    {
        this.dataset = new List<TbFinancialAccount>
        {
            new () { Id = Guid.NewGuid(), Name = "Savings" },
            new () { Id = Guid.NewGuid(), Name = "Bank account" },
            new () { Id = Guid.NewGuid(), Name = "Cash" },
            new () { Id = Guid.NewGuid(), Name = "Empty account" }
        };
        this.mapperConfig = new TypeAdapterConfig();
        new FinancialAccountBalanceSummaryMapsterConfig().Register(this.mapperConfig);
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();
        this.permissionServiceMock
            .Setup(x => x.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ViewFinancialAccountBalances,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbFinancialAccount, FinancialAccountBalanceSummary>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>, CancellationToken>((query, _) =>
                Task.FromResult<IReadOnlyCollection<FinancialAccountBalanceSummary>>(
                    query(this.dataset.AsQueryable()).ProjectToType<FinancialAccountBalanceSummary>(this.mapperConfig).ToList()));
        this.service = new FinancialAccountBalanceSummaryService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithSettledTransactions_ReturnsNetBalanceForEachAccountAsync()
    {
        AddReceipt(this.dataset[1], 125.25m);
        AddReceipt(this.dataset[1], 74.85m);
        AddPayment(this.dataset[1], 20.05m);
        AddPayment(this.dataset[1], 50m);
        AddPayment(this.dataset[2], 12.34m);
        AddReceipt(this.dataset[0], 10m);
        AddPayment(this.dataset[0], 10m);

        var response = await this.service.GetSummariesAsync(new SearchRequest());

        response.Summaries.Select(x => x.Name).ShouldBe(new[] { "Bank account", "Cash", "Empty account", "Savings" });
        response.Summaries.Select(x => x.Balance).ShouldBe(new[] { 130.05m, -12.34m, 0m, 0m });
    }

    [Test]
    public async Task GetSummaries_WithPendingTransactions_ExcludesTheirAmountsAsync()
    {
        AddReceipt(this.dataset[1], 100m);
        AddPayment(this.dataset[1], 30m);
        AddReceipt(this.dataset[1], 1000m, settled: false);
        AddPayment(this.dataset[1], 2000m, settled: false);

        var response = await this.service.GetSummariesAsync(new SearchRequest());

        response.Summaries.Single(x => x.Id == this.dataset[1].Id).Balance.ShouldBe(70m);
    }

    [Test]
    public async Task GetSummaries_WithoutDateFilters_IncludesAllSettlementDatesAsync()
    {
        AddReceipt(this.dataset[1], 150m, date: new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
        AddReceipt(this.dataset[1], 50m, date: new DateTimeOffset(2100, 12, 31, 0, 0, 0, TimeSpan.Zero));
        AddPayment(this.dataset[1], 25m, date: new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
        AddPayment(this.dataset[1], 10m, date: new DateTimeOffset(2100, 12, 31, 0, 0, 0, TimeSpan.Zero));

        var response = await this.service.GetSummariesAsync(new SearchRequest());

        response.Summaries.Single(x => x.Id == this.dataset[1].Id).Balance.ShouldBe(165m);
    }

    [Test]
    public async Task GetSummaries_WithTrimmedSearchText_FiltersByNameAsync()
    {
        var response = await this.service.GetSummariesAsync(new SearchRequest { SearchText = "  Bank  " });

        response.Summaries.Select(x => x.Id).ShouldBe(new[] { this.dataset[1].Id });
    }

    [Test]
    public async Task GetSummaries_WithTake_OrdersBeforeApplyingLimitAsync()
    {
        var response = await this.service.GetSummariesAsync(new SearchRequest { Take = 2 });

        response.Summaries.Select(x => x.Name).ShouldBe(new[] { "Bank account", "Cash" });
    }

    [Test]
    public async Task GetSummaries_WhenNoAccountMatches_ReturnsEmptyCollectionAsync()
    {
        var response = await this.service.GetSummariesAsync(new SearchRequest { SearchText = "Unknown account" });

        response.Summaries.ShouldBeEmpty();
    }

    [Test]
    public async Task GetSummaries_WhenPermissionDenied_DoesNotAccessDatabaseAsync()
    {
        this.permissionServiceMock
            .Setup(x => x.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ViewFinancialAccountBalances,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AppException(ExceptionCode.SecurityValidation));

        var exception = await AssertAppExceptionAsync(() => this.service.GetSummariesAsync(new SearchRequest()));

        exception.ExceptionCode.ShouldBe(ExceptionCode.SecurityValidation);
        this.databaseRepositoryMock.Invocations.ShouldBeEmpty();
    }

    [Test]
    public async Task GetSummaries_ForwardsCancellationAndValidatesPermissionAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        await this.service.GetSummariesAsync(new SearchRequest(), cancellationTokenSource.Token);

        this.permissionServiceMock.Verify(
            x => x.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ViewFinancialAccountBalances,
                null,
                cancellationTokenSource.Token),
            Times.Once);
        this.databaseRepositoryMock.Verify(
            x => x.GetByConditionAsync<TbFinancialAccount, FinancialAccountBalanceSummary>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                cancellationTokenSource.Token),
            Times.Once);
    }

    private static void AddReceipt(TbFinancialAccount account, decimal amount, bool settled = true, DateTimeOffset? date = null)
    {
        account.AccountsReceivable.Add(new TbAccountReceivable
        {
            Id = Guid.NewGuid(),
            FinancialAccountId = account.Id,
            FinancialAccount = account,
            Amount = amount,
            IsReceived = settled,
            ReceiptDate = date ?? DateTimeOffset.UtcNow
        });
    }

    private static void AddPayment(TbFinancialAccount account, decimal amount, bool settled = true, DateTimeOffset? date = null)
    {
        account.AccountsPayable.Add(new TbAccountPayable
        {
            Id = Guid.NewGuid(),
            FinancialAccountId = account.Id,
            FinancialAccount = account,
            Amount = amount,
            IsPaid = settled,
            PaymentDate = date ?? DateTimeOffset.UtcNow
        });
    }

    private static async Task<AppException> AssertAppExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
            Assert.Fail("Expected AppException was not thrown.");
            throw new InvalidOperationException();
        }
        catch (AppException ex)
        {
            return ex;
        }
    }
}
