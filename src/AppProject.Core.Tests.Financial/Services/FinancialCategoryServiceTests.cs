using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Financial;
using AppProject.Core.Models.Financial;
using AppProject.Core.Services.Auth;
using AppProject.Core.Services.Financial;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Bogus;
using Moq;
using Shouldly;

namespace AppProject.Core.Tests.Financial.Services;

[TestFixture]
public class FinancialCategoryServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private FinancialCategoryService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();

        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialCategories,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.service = new FinancialCategoryService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetEntity_WhenFinancialCategoryExists_ReturnsEntityAsync()
    {
        var id = Guid.NewGuid();
        var expectedFinancialCategory = new FinancialCategory
        {
            Id = id,
            Name = this.faker.Commerce.Department(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory, FinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFinancialCategory);

        var response = await this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = id });

        response.Entity.ShouldBe(expectedFinancialCategory);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialCategories,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetEntity_WhenFinancialCategoryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory, FinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialCategory?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.GetEntityAsync(
            new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task PostEntity_WhenFinancialCategoryNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var financialCategory = new FinancialCategory
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Commerce.Department(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
            new CreateOrUpdateRequest<FinancialCategory> { Entity = financialCategory }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.Financial_FinancialCategory_DuplicateName);
    }

    [Test]
    public async Task PostEntity_WhenValid_PersistsFinancialCategoryAsync()
    {
        var id = Guid.NewGuid();
        var financialCategory = new FinancialCategory
        {
            Id = id,
            Name = this.faker.Commerce.Department(),
        };

        TbFinancialCategory? insertedFinancialCategory = null;

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.InsertAndSaveAsync(
                It.IsAny<TbFinancialCategory>(),
                It.IsAny<CancellationToken>()))
            .Callback<TbFinancialCategory, CancellationToken>((entity, _) => insertedFinancialCategory = entity)
            .Returns(Task.CompletedTask);

        var response = await this.service.PostEntityAsync(new CreateOrUpdateRequest<FinancialCategory> { Entity = financialCategory });

        response.Id.ShouldBe(id);
        insertedFinancialCategory.ShouldNotBe(null);
        insertedFinancialCategory!.Name.ShouldBe(financialCategory.Name);

        this.databaseRepositoryMock.Verify(
            x => x.InsertAndSaveAsync(
                It.Is<TbFinancialCategory>(tb => tb.Id == id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task PutEntity_WhenFinancialCategoryNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var financialCategory = new FinancialCategory
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Commerce.Department(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<FinancialCategory> { Entity = financialCategory }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.Financial_FinancialCategory_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenFinancialCategoryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var financialCategory = new FinancialCategory
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Commerce.Department(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbFinancialCategory?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<FinancialCategory> { Entity = financialCategory }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenFinancialCategoryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var id = Guid.NewGuid();

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbFinancialCategory?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.DeleteEntityAsync(
            new DeleteRequest<Guid> { Id = id }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenFinancialCategoryExists_RemovesFinancialCategoryAsync()
    {
        var id = Guid.NewGuid();
        var entity = new TbFinancialCategory { Id = id };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        this.databaseRepositoryMock
            .Setup(x => x.DeleteAndSaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await this.service.DeleteEntityAsync(new DeleteRequest<Guid> { Id = id });

        this.databaseRepositoryMock.Verify(
            x => x.DeleteAndSaveAsync(
                entity,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task DeleteEntity_WhenLinkedToTransactions_ThrowsInUseAsync(bool hasPayable)
    {
        var entity = new TbFinancialCategory { Id = Guid.NewGuid() };
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbAccountPayable>(
                It.IsAny<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasPayable);
        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbAccountReceivable>(
                It.IsAny<Func<IQueryable<TbAccountReceivable>, IQueryable<TbAccountReceivable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(!hasPayable);

        var exception = await AssertAppExceptionAsync(() => this.service.DeleteEntityAsync(
            new DeleteRequest<Guid> { Id = entity.Id }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.Financial_FinancialCategory_InUse);
        this.databaseRepositoryMock.Verify(
            x => x.DeleteAndSaveAsync(It.IsAny<TbFinancialCategory>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestCase("Get")]
    [TestCase("Post")]
    [TestCase("Put")]
    [TestCase("Delete")]
    public async Task Operation_WhenPermissionDenied_DoesNotAccessDatabaseAsync(string operation)
    {
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialCategories,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AppException(ExceptionCode.SecurityValidation));
        var id = Guid.NewGuid();
        var request = new CreateOrUpdateRequest<FinancialCategory>
        {
            Entity = new FinancialCategory { Id = id, Name = "Rent" }
        };
        Func<Task> action = operation switch
        {
            "Get" => () => this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = id }),
            "Post" => () => this.service.PostEntityAsync(request),
            "Put" => () => this.service.PutEntityAsync(request),
            "Delete" => () => this.service.DeleteEntityAsync(new DeleteRequest<Guid> { Id = id }),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        var exception = await AssertAppExceptionAsync(action);

        exception.ExceptionCode.ShouldBe(ExceptionCode.SecurityValidation);
        this.databaseRepositoryMock.Invocations.ShouldBeEmpty();
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
