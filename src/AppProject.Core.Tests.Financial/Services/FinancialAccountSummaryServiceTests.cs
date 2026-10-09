using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Core.Services.Financial;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Moq;
using Shouldly;

namespace AppProject.Core.Tests.Financial.Services;

[TestFixture]
public class FinancialAccountSummaryServiceTests
{
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private FinancialAccountSummaryService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialAccounts,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.service = new FinancialAccountSummaryService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithTrimmedSearchText_FiltersByNameAsync()
    {
        var targetFinancialAccount = new TbFinancialAccount
        {
            Id = Guid.NewGuid(),
            Name = "Bank account",
        };

        var dataset = new List<TbFinancialAccount>
        {
            targetFinancialAccount,
            new () { Id = Guid.NewGuid(), Name = "Cash" },
            new () { Id = Guid.NewGuid(), Name = "Savings" }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbFinancialAccount, FinancialAccountSummary>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(financialAccount => new FinancialAccountSummary
                {
                    Id = financialAccount.Id,
                    Name = financialAccount.Name
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<FinancialAccountSummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new SearchRequest { SearchText = "  Bank " });

        response.Summaries.Count.ShouldBe(1);
        var summary = response.Summaries.Single();
        summary.Id.ShouldBe(targetFinancialAccount.Id);
    }

    [Test]
    public async Task GetSummaries_WithTakeValue_RespectsOrderingAndLimitAsync()
    {
        var dataset = new List<TbFinancialAccount>
        {
            new () { Id = Guid.NewGuid(), Name = "Savings" },
            new () { Id = Guid.NewGuid(), Name = "Bank account" },
            new () { Id = Guid.NewGuid(), Name = "Checking account" }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbFinancialAccount, FinancialAccountSummary>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(financialAccount => new FinancialAccountSummary
                {
                    Id = financialAccount.Id,
                    Name = financialAccount.Name
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<FinancialAccountSummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new SearchRequest { Take = 2 });

        response.Summaries.Count.ShouldBe(2);
        response.Summaries.First().Name.ShouldBe("Bank account");
        response.Summaries.Last().Name.ShouldBe("Checking account");
    }

    [Test]
    public async Task GetSummary_WhenFinancialAccountDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount, FinancialAccountSummary>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialAccountSummary?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.GetSummaryAsync(
            new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Read_WhenPermissionDenied_DoesNotAccessDatabaseAsync(bool list)
    {
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialAccounts,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AppException(ExceptionCode.SecurityValidation));
        Func<Task> action = list
            ? () => this.service.GetSummariesAsync(new SearchRequest())
            : () => this.service.GetSummaryAsync(new GetByIdRequest<Guid> { Id = Guid.NewGuid() });

        var exception = await AssertAppExceptionAsync(action);

        exception.ExceptionCode.ShouldBe(ExceptionCode.SecurityValidation);
        this.databaseRepositoryMock.Invocations.ShouldBeEmpty();
    }

    [Test]
    public async Task GetSummary_WhenAccountExists_ReturnsRequestedAccountAsync()
    {
        var expected = new FinancialAccountSummary { Id = Guid.NewGuid(), Name = "Checking account" };
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount, FinancialAccountSummary>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await this.service.GetSummaryAsync(new GetByIdRequest<Guid> { Id = expected.Id });

        response.Summary.ShouldBe(expected);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialAccounts,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
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
