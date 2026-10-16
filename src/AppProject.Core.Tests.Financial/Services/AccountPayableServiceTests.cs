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
public class AccountPayableServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private AccountPayableService service = null!;
    private Guid categoryId;
    private Guid financialAccountId;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.categoryId = Guid.NewGuid();
        this.financialAccountId = Guid.NewGuid();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageAccountsPayable,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialCategory>(
                It.IsAny<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbFinancialCategory>, IQueryable<TbFinancialCategory>>, CancellationToken>(
                (query, _) => Task.FromResult(query(new[] { new TbFinancialCategory { Id = this.categoryId } }.AsQueryable()).Any()));
        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbFinancialAccount>(
                It.IsAny<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbFinancialAccount>, IQueryable<TbFinancialAccount>>, CancellationToken>(
                (query, _) => Task.FromResult(query(new[] { new TbFinancialAccount { Id = this.financialAccountId } }.AsQueryable()).Any()));
        this.service = new AccountPayableService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetEntity_WhenEntityExists_ReturnsEntityAndForwardsCancellationAsync()
    {
        var expected = this.CreateValidEntity();
        using var cancellation = new CancellationTokenSource();
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbAccountPayable, AccountPayable>(
                It.IsAny<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>>(),
                cancellation.Token))
            .ReturnsAsync(expected);

        var response = await this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = expected.Id!.Value }, cancellation.Token);

        response.Entity.ShouldBe(expected);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(PermissionType.Financial_ManageAccountsPayable, null, cancellation.Token),
            Times.Once);
    }

    [TestCase(0.01)]
    [TestCase(150)]
    public async Task PostEntity_WhenPendingAndPositive_PersistsEntityAsync(decimal amount)
    {
        var request = this.CreateValidEntity();
        request.Amount = amount;
        TbAccountPayable? inserted = null;
        this.databaseRepositoryMock
            .Setup(x => x.InsertAndSaveAsync(It.IsAny<TbAccountPayable>(), It.IsAny<CancellationToken>()))
            .Callback<TbAccountPayable, CancellationToken>((entity, _) => inserted = entity)
            .Returns(Task.CompletedTask);

        var response = await this.service.PostEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request });

        response.Id.ShouldBe(request.Id!.Value);
        inserted.ShouldNotBeNull();
        inserted!.FinancialCategoryId.ShouldBe(request.FinancialCategoryId);
        inserted.Description.ShouldBe(request.Description);
        inserted.Amount.ShouldBe(amount);
        inserted.DueDate.ShouldBe(request.DueDate);
        inserted.IsPaid.ShouldBeFalse();
        inserted.PaymentDate.ShouldBeNull();
        inserted.FinancialAccountId.ShouldBeNull();
        this.databaseRepositoryMock.Verify(x => x.InsertAndSaveAsync(inserted, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PostEntity_WhenSettled_PersistsSettlementFieldsAsync()
    {
        var request = this.CreateValidEntity();
        request.IsPaid = true;
        request.FinancialAccountId = this.financialAccountId;
        request.PaymentDate = request.DueDate;
        TbAccountPayable? inserted = null;
        this.databaseRepositoryMock
            .Setup(x => x.InsertAndSaveAsync(It.IsAny<TbAccountPayable>(), It.IsAny<CancellationToken>()))
            .Callback<TbAccountPayable, CancellationToken>((entity, _) => inserted = entity)
            .Returns(Task.CompletedTask);

        await this.service.PostEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request });

        inserted.ShouldNotBeNull();
        inserted!.IsPaid.ShouldBeTrue();
        inserted.PaymentDate.ShouldBe(request.PaymentDate);
        inserted.FinancialAccountId.ShouldBe(this.financialAccountId);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task PutEntity_WhenChangingSettlement_PersistsRequestedStateAsync(bool settled)
    {
        var request = this.CreateValidEntity();
        request.IsPaid = settled;
        request.FinancialAccountId = settled ? this.financialAccountId : null;
        request.PaymentDate = settled ? request.DueDate : null;
        var entity = this.SetExistingEntity(request);
        entity.IsPaid = !settled;
        entity.Description = "Previous description";

        var response = await this.service.PutEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request });

        response.Id.ShouldBe(entity.Id);
        entity.Description.ShouldBe(request.Description);
        entity.Amount.ShouldBe(request.Amount);
        entity.DueDate.ShouldBe(request.DueDate);
        entity.FinancialCategoryId.ShouldBe(request.FinancialCategoryId);
        entity.RowVersion.ShouldBe(request.RowVersion);
        entity.IsPaid.ShouldBe(settled);
        entity.PaymentDate.ShouldBe(request.PaymentDate);
        entity.FinancialAccountId.ShouldBe(request.FinancialAccountId);
        this.databaseRepositoryMock.Verify(x => x.UpdateAndSaveAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase("MissingAccount", false, ExceptionCode.Financial_AccountPayable_FinancialAccountRequired)]
    [TestCase("MissingAccount", true, ExceptionCode.Financial_AccountPayable_FinancialAccountRequired)]
    [TestCase("EmptyAccount", false, ExceptionCode.Financial_AccountPayable_FinancialAccountRequired)]
    [TestCase("EmptyAccount", true, ExceptionCode.Financial_AccountPayable_FinancialAccountRequired)]
    [TestCase("MissingSettlementDate", false, ExceptionCode.Financial_AccountPayable_PaymentDateRequired)]
    [TestCase("MissingSettlementDate", true, ExceptionCode.Financial_AccountPayable_PaymentDateRequired)]
    public async Task Save_WhenBusinessRuleFails_DoesNotPersistAsync(string scenario, bool update, ExceptionCode expected)
    {
        var request = this.CreateValidEntity();
        this.SetExistingEntity(request);
        switch (scenario)
        {
            case "MissingAccount":
                request.IsPaid = true;
                break;
            case "EmptyAccount":
                request.IsPaid = true;
                request.FinancialAccountId = Guid.Empty;
                break;
            case "MissingSettlementDate":
                request.IsPaid = true;
                request.FinancialAccountId = this.financialAccountId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var exception = await AssertAppExceptionAsync(() => update
            ? this.service.PutEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request })
            : this.service.PostEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request }));

        exception.ExceptionCode.ShouldBe(expected);
        this.VerifyNotSaved();
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Save_WhenReferenceDoesNotExist_DoesNotPersistAsync(bool missingCategory, bool update)
    {
        var request = this.CreateValidEntity();
        this.SetExistingEntity(request);
        if (missingCategory)
        {
            request.FinancialCategoryId = Guid.NewGuid();
        }
        else
        {
            request.FinancialAccountId = Guid.NewGuid();
        }

        var exception = await AssertAppExceptionAsync(() => update
            ? this.service.PutEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request })
            : this.service.PostEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request }));

        exception.ExceptionCode.ShouldBe(missingCategory
            ? ExceptionCode.Financial_AccountPayable_FinancialCategoryNotFound
            : ExceptionCode.Financial_AccountPayable_FinancialAccountNotFound);
        this.VerifyNotSaved();
    }

    [TestCase("Get")]
    [TestCase("Put")]
    [TestCase("Delete")]
    public async Task Operation_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync(string operation)
    {
        var request = this.CreateValidEntity();
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbAccountPayable, AccountPayable>(
                It.IsAny<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountPayable?)null);
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbAccountPayable>(
                It.IsAny<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbAccountPayable?)null);

        var exception = await AssertAppExceptionAsync(this.CreateAction(operation, request));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
        this.VerifyNotSaved();
        this.databaseRepositoryMock.Verify(x => x.DeleteAndSaveAsync(It.IsAny<TbAccountPayable>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteEntity_WhenEntityExists_RemovesEntityAsync()
    {
        var request = this.CreateValidEntity();
        var entity = this.SetExistingEntity(request);

        await this.service.DeleteEntityAsync(new DeleteRequest<Guid> { Id = entity.Id });

        this.databaseRepositoryMock.Verify(x => x.DeleteAndSaveAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase("Get")]
    [TestCase("Post")]
    [TestCase("Put")]
    [TestCase("Delete")]
    public async Task Operation_WhenPermissionDenied_DoesNotAccessDatabaseAsync(string operation)
    {
        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.Financial_ManageAccountsPayable,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AppException(ExceptionCode.SecurityValidation));

        var exception = await AssertAppExceptionAsync(this.CreateAction(operation, this.CreateValidEntity()));

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

    private AccountPayable CreateValidEntity() => new ()
    {
        Id = Guid.NewGuid(),
        FinancialCategoryId = this.categoryId,
        Description = this.faker.Commerce.ProductName(),
        Amount = 100m,
        DueDate = new DateTimeOffset(2030, 1, 10, 12, 0, 0, TimeSpan.FromHours(-3)),
        RowVersion = new byte[] { 1, 2, 3 }
    };

    private TbAccountPayable SetExistingEntity(AccountPayable request)
    {
        var entity = new TbAccountPayable
        {
            Id = request.Id!.Value,
            RowVersion = request.RowVersion!.ToArray()
        };
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbAccountPayable>(
                It.IsAny<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbAccountPayable>, IQueryable<TbAccountPayable>>, CancellationToken>(
                (query, _) => Task.FromResult(query(new[] { entity }.AsQueryable()).FirstOrDefault()));
        this.databaseRepositoryMock
            .Setup(x => x.UpdateAndSaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.databaseRepositoryMock
            .Setup(x => x.DeleteAndSaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return entity;
    }

    private Func<Task> CreateAction(string operation, AccountPayable request) => operation switch
    {
        "Get" => () => this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = request.Id!.Value }),
        "Post" => () => this.service.PostEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request }),
        "Put" => () => this.service.PutEntityAsync(new CreateOrUpdateRequest<AccountPayable> { Entity = request }),
        "Delete" => () => this.service.DeleteEntityAsync(new DeleteRequest<Guid> { Id = request.Id!.Value }),
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    private void VerifyNotSaved()
    {
        this.databaseRepositoryMock.Verify(x => x.InsertAndSaveAsync(It.IsAny<TbAccountPayable>(), It.IsAny<CancellationToken>()), Times.Never);
        this.databaseRepositoryMock.Verify(x => x.UpdateAndSaveAsync(It.IsAny<TbAccountPayable>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
