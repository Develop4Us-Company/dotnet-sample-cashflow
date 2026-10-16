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
public class FinancialAccountServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private FinancialAccountService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();

        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialAccounts,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.service = new FinancialAccountService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetEntity_WhenFinancialAccountExists_ReturnsEntityAsync()
    {
        var id = Guid.NewGuid();
        var expectedFinancialAccount = new FinancialAccount
        {
            Id = id,
            Name = this.faker.Finance.AccountName(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount, FinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFinancialAccount);

        var response = await this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = id });

        response.Entity.ShouldBe(expectedFinancialAccount);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageFinancialAccounts,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetEntity_WhenFinancialAccountDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount, FinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialAccount?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.GetEntityAsync(
            new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task PostEntity_WhenFinancialAccountNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var financialAccount = new FinancialAccount
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Finance.AccountName(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
            new CreateOrUpdateRequest<FinancialAccount> { Entity = financialAccount }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.Financial_FinancialAccount_DuplicateName);
    }

    [Test]
    public async Task PostEntity_WhenValid_PersistsFinancialAccountAsync()
    {
        var id = Guid.NewGuid();
        var financialAccount = new FinancialAccount
        {
            Id = id,
            Name = this.faker.Finance.AccountName(),
        };

        TbFinancialAccount? insertedFinancialAccount = null;

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.InsertAndSaveAsync(
                It.IsAny<TbFinancialAccount>(),
                It.IsAny<CancellationToken>()))
            .Callback<TbFinancialAccount, CancellationToken>((entity, _) => insertedFinancialAccount = entity)
            .Returns(Task.CompletedTask);

        var response = await this.service.PostEntityAsync(new CreateOrUpdateRequest<FinancialAccount> { Entity = financialAccount });

        response.Id.ShouldBe(id);
        insertedFinancialAccount.ShouldNotBe(null);
        insertedFinancialAccount!.Name.ShouldBe(financialAccount.Name);

        this.databaseRepositoryMock.Verify(
            x => x.InsertAndSaveAsync(
                It.Is<TbFinancialAccount>(tb => tb.Id == id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task PutEntity_WhenFinancialAccountNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var financialAccount = new FinancialAccount
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Finance.AccountName(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<FinancialAccount> { Entity = financialAccount }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.Financial_FinancialAccount_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenFinancialAccountDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var financialAccount = new FinancialAccount
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Finance.AccountName(),
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbFinancialAccount?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<FinancialAccount> { Entity = financialAccount }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenFinancialAccountDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var id = Guid.NewGuid();

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbFinancialAccount?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.DeleteEntityAsync(
            new DeleteRequest<Guid> { Id = id }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenFinancialAccountExists_RemovesFinancialAccountAsync()
    {
        var id = Guid.NewGuid();
        var entity = new TbFinancialAccount { Id = id };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
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
        var entity = new TbFinancialAccount { Id = Guid.NewGuid() };
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
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

        exception.ExceptionCode.ShouldBe(ExceptionCode.Financial_FinancialAccount_InUse);
        this.databaseRepositoryMock.Verify(
            x => x.DeleteAndSaveAsync(It.IsAny<TbFinancialAccount>(), It.IsAny<CancellationToken>()),
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
                PermissionType.Financial_ManageFinancialAccounts,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AppException(ExceptionCode.SecurityValidation));
        var id = Guid.NewGuid();
        var request = new CreateOrUpdateRequest<FinancialAccount>
        {
            Entity = new FinancialAccount { Id = id, Name = "Checking account" }
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
