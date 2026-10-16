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
public class AccountPayableSummaryServiceTests
{
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private AccountPayableSummaryService service = null!;
    private List<TbAccountPayable> dataset = null!;
    private DateTimeOffset dueDate;
    private Guid financialAccountId;

    [SetUp]
    public void SetUp()
    {
        this.dueDate = new DateTimeOffset(2030, 1, 10, 12, 0, 0, TimeSpan.FromHours(-3));
        this.financialAccountId = Guid.NewGuid();
        this.dataset = new List<TbAccountPayable>
        {
            this.CreateEntity("Invoice A", 0, true, this.financialAccountId),
            this.CreateEntity("Invoice B", 1, false, null),
            this.CreateEntity("Invoice C", 2, true, Guid.NewGuid()),
            this.CreateEntity("Unrelated", 3, false, this.financialAccountId),
            this.CreateEntity("Invoice E", -1, false, this.financialAccountId)
        };
        foreach (var entity in this.dataset)
        {
            entity.FinancialCategoryId = entity.FinancialCategory.Id;
        }

        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageAccountsPayable,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbAccountPayable, AccountPayableSummary>(
                It.IsAny<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>, CancellationToken>((query, _) =>
                Task.FromResult<IReadOnlyCollection<AccountPayableSummary>>(query(this.dataset.AsQueryable()).Select(ToSummary).ToList()));
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbAccountPayable, AccountPayableSummary>(
                It.IsAny<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>, CancellationToken>((query, _) =>
                Task.FromResult(query(this.dataset.AsQueryable()).AsEnumerable().Select(ToSummary).FirstOrDefault()));
        this.service = new AccountPayableSummaryService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithTrimmedSearchText_SearchesOnlyDescriptionAsync()
    {
        this.dataset[3].FinancialCategory.Name = "Invoice";
        this.dataset[3].FinancialAccount!.Name = "Invoice";

        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest { SearchText = "  Invoice " });

        response.Summaries.Select(x => x.Description).ShouldBe(new[] { "Invoice E", "Invoice A", "Invoice B", "Invoice C" });
    }

    [TestCase("From")]
    [TestCase("To")]
    [TestCase("Range")]
    [TestCase("Exact")]
    public async Task GetSummaries_WithDateBounds_IncludesEndpointsAsync(string bounds)
    {
        var request = new AccountPayableSummarySearchRequest();
        string[] expected;
        switch (bounds)
        {
            case "From":
                request.DueDateFrom = this.dueDate;
                expected = new[] { "Invoice A", "Invoice B", "Invoice C", "Unrelated" };
                break;
            case "To":
                request.DueDateTo = this.dueDate;
                expected = new[] { "Invoice E", "Invoice A" };
                break;
            case "Range":
                request.DueDateFrom = this.dueDate;
                request.DueDateTo = this.dueDate.AddDays(2);
                expected = new[] { "Invoice A", "Invoice B", "Invoice C" };
                break;
            case "Exact":
                request.DueDateFrom = this.dueDate.ToUniversalTime();
                request.DueDateTo = this.dueDate.ToUniversalTime();
                expected = new[] { "Invoice A" };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(bounds));
        }

        var response = await this.service.GetSummariesAsync(request);

        response.Summaries.Select(x => x.Description).ShouldBe(expected);
    }

    [TestCase(true)]
    [TestCase(false)]
    [TestCase(null)]
    public async Task GetSummaries_WithOptionalStatus_FiltersCorrectlyAsync(bool? settled)
    {
        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest { IsPaid = settled });

        var expected = this.dataset
            .Where(x => !settled.HasValue || x.IsPaid == settled.Value)
            .OrderBy(x => x.DueDate)
            .Select(x => x.Id);
        response.Summaries.Select(x => x.Id).ShouldBe(expected);
    }

    [Test]
    public async Task GetSummaries_WithAccountFilter_ExcludesOtherAndMissingAccountsAsync()
    {
        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest { FinancialAccountId = this.financialAccountId });

        response.Summaries.Select(x => x.Description).ShouldBe(new[] { "Invoice E", "Invoice A", "Unrelated" });
    }

    [Test]
    public async Task GetSummaries_WithCombinedFilters_IntersectsAllFiltersAsync()
    {
        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest
        {
            SearchText = "Invoice",
            FinancialCategoryId = this.dataset[0].FinancialCategoryId,
            DueDateFrom = this.dueDate,
            DueDateTo = this.dueDate.AddDays(2),
            IsPaid = true,
            FinancialAccountId = this.financialAccountId
        });

        response.Summaries.Select(x => x.Id).ShouldBe(new[] { this.dataset[0].Id });
    }

    [Test]
    public async Task GetSummaries_WithCategoryFilter_ExcludesOtherCategoriesAsync()
    {
        this.dataset[2].FinancialCategory = this.dataset[0].FinancialCategory;
        this.dataset[2].FinancialCategoryId = this.dataset[0].FinancialCategoryId;

        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest
        {
            FinancialCategoryId = this.dataset[0].FinancialCategoryId
        });

        response.Summaries.Select(x => x.Description).ShouldBe(new[] { "Invoice A", "Invoice C" });
    }

    [Test]
    public async Task GetSummaries_WithUnmatchedCategory_ReturnsEmptyCollectionAsync()
    {
        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest
        {
            FinancialCategoryId = Guid.NewGuid()
        });

        response.Summaries.ShouldBeEmpty();
    }

    [Test]
    public async Task GetSummaries_WithTake_OrdersBeforeApplyingLimitAsync()
    {
        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest { Take = 2 });

        response.Summaries.Select(x => x.Description).ShouldBe(new[] { "Invoice E", "Invoice A" });
    }

    [Test]
    public async Task GetSummaries_WhenNothingMatches_ReturnsEmptyCollectionAsync()
    {
        var response = await this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest { SearchText = "No match" });

        response.Summaries.ShouldBeEmpty();
    }

    [Test]
    public async Task GetSummaries_WhenDateRangeIsInverted_DoesNotAccessDatabaseAsync()
    {
        var exception = await AssertAppExceptionAsync(() => this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest
        {
            DueDateFrom = this.dueDate.AddDays(1),
            DueDateTo = this.dueDate
        }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.Financial_AccountPayable_InvalidDueDateRange);
        this.databaseRepositoryMock.Invocations.ShouldBeEmpty();
    }

    [Test]
    public async Task GetSummary_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var exception = await AssertAppExceptionAsync(() => this.service.GetSummaryAsync(new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Read_WhenPermissionDenied_DoesNotAccessDatabaseAsync(bool list)
    {
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageAccountsPayable,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AppException(ExceptionCode.SecurityValidation));
        Func<Task> action = list
            ? () => this.service.GetSummariesAsync(new AccountPayableSummarySearchRequest())
            : () => this.service.GetSummaryAsync(new GetByIdRequest<Guid> { Id = Guid.NewGuid() });

        var exception = await AssertAppExceptionAsync(action);

        exception.ExceptionCode.ShouldBe(ExceptionCode.SecurityValidation);
        this.databaseRepositoryMock.Invocations.ShouldBeEmpty();
    }

    private static AccountPayableSummary ToSummary(TbAccountPayable entity) => new ()
    {
        Id = entity.Id,
        FinancialCategoryName = entity.FinancialCategory.Name,
        FinancialAccountName = entity.FinancialAccount?.Name,
        Description = entity.Description,
        Amount = entity.Amount,
        DueDate = entity.DueDate,
        IsPaid = entity.IsPaid,
        PaymentDate = entity.PaymentDate
    };

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

    private TbAccountPayable CreateEntity(string description, int dayOffset, bool settled, Guid? accountId) => new ()
    {
        Id = Guid.NewGuid(),
        Description = description,
        Amount = 100m,
        DueDate = this.dueDate.AddDays(dayOffset),
        IsPaid = settled,
        PaymentDate = settled ? this.dueDate : null,
        FinancialCategory = new TbFinancialCategory { Id = Guid.NewGuid(), Name = "Category" },
        FinancialAccountId = accountId,
        FinancialAccount = accountId.HasValue ? new TbFinancialAccount { Id = accountId.Value, Name = "Account" } : null
    };
}
