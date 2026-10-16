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
public class FinancialCategorySummaryServiceTests
{
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private FinancialCategorySummaryService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialCategories,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.service = new FinancialCategorySummaryService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithTrimmedSearchText_FiltersByNameAsync()
    {
        var targetFinancialCategory = new TbFinancialCategory
        {
            Id = Guid.NewGuid(),
            Name = "Housing",
        };

        var dataset = new List<TbFinancialCategory>
        {
            targetFinancialCategory,
            new () { Id = Guid.NewGuid(), Name = "Food" },
            new () { Id = Guid.NewGuid(), Name = "Transport" }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbFinancialCategory, FinancialCategorySummary>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(financialCategory => new FinancialCategorySummary
                {
                    Id = financialCategory.Id,
                    Name = financialCategory.Name
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<FinancialCategorySummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new SearchRequest { SearchText = "  Housing " });

        response.Summaries.Count.ShouldBe(1);
        var summary = response.Summaries.Single();
        summary.Id.ShouldBe(targetFinancialCategory.Id);
    }

    [Test]
    public async Task GetSummaries_WithTakeValue_RespectsOrderingAndLimitAsync()
    {
        var dataset = new List<TbFinancialCategory>
        {
            new () { Id = Guid.NewGuid(), Name = "Transport" },
            new () { Id = Guid.NewGuid(), Name = "Housing" },
            new () { Id = Guid.NewGuid(), Name = "Rent" }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbFinancialCategory, FinancialCategorySummary>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(financialCategory => new FinancialCategorySummary
                {
                    Id = financialCategory.Id,
                    Name = financialCategory.Name
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<FinancialCategorySummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new SearchRequest { Take = 2 });

        response.Summaries.Count.ShouldBe(2);
        response.Summaries.First().Name.ShouldBe("Housing");
        response.Summaries.Last().Name.ShouldBe("Rent");
    }

    [Test]
    public async Task GetSummary_WhenFinancialCategoryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory, FinancialCategorySummary>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialCategorySummary?)null);

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
                PermissionType.Financial_ManageFinancialCategories,
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
    public async Task GetSummary_WhenCategoryExists_ReturnsRequestedCategoryAsync()
    {
        var expected = new FinancialCategorySummary { Id = Guid.NewGuid(), Name = "Rent" };
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory, FinancialCategorySummary>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await this.service.GetSummaryAsync(new GetByIdRequest<Guid> { Id = expected.Id });

        response.Summary.ShouldBe(expected);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialCategories,
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
