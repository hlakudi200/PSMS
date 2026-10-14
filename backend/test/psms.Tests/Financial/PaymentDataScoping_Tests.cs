using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Domain.Academic.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Financial.PaymentAllocations;
using psms.Financial.Payments;
using psms.Financial.Payments.Dto;
using psms.Financial.Shared;
using Shouldly;
using Xunit;

namespace psms.Tests.Financial;

/// <summary>
/// MOB-BE-04 follow-up: parents hold <c>Financial.Payments.View</c> and
/// <c>Financial.Payments.Process</c>, and the payment services never asked
/// whose payment it was. A parent could read any family's payments — by id, by
/// learner, by parent, or by counting through the sequential receipt numbers —
/// and could mark a pending payment as paid.
/// <para>
/// Family A: parent A ↔ Ann, with a completed payment REC-000001 allocated to
/// her fee. Family B: parent B ↔ Ben, with a completed REC-000002 and a
/// pending EFT REC-000003.
/// </para>
/// </summary>
public class PaymentDataScoping_Tests : psmsTestBase
{
    private readonly IPaymentAppService _payments;
    private readonly IPaymentAllocationAppService _allocations;

    private Guid _annId;
    private Guid _benId;
    private Guid _parentAId;
    private Guid _parentBId;
    private Guid _annPaymentId;
    private Guid _benPaymentId;
    private Guid _benPendingPaymentId;
    private Guid _benFeeId;
    private Guid _benAllocationId;
    private long _parentAUserId;
    private long _financeUserId;

    public PaymentDataScoping_Tests()
    {
        _payments = Resolve<IPaymentAppService>();
        _allocations = Resolve<IPaymentAllocationAppService>();
        Seed();
    }

    private void Seed()
    {
        var parentAUser = CreateUser("pay.parent.a");
        var parentBUser = CreateUser("pay.parent.b");
        var financeUser = CreateUser("pay.finance");
        _parentAUserId = parentAUser.Id;
        _financeUserId = financeUser.Id;

        AssignRole(parentAUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(parentBUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(financeUser.Id, StaticRoleNames.Tenants.Finance);

        var yearId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var classId = Guid.NewGuid();
        var structureId = Guid.NewGuid();
        _annId = Guid.NewGuid();
        _benId = Guid.NewGuid();
        _parentAId = Guid.NewGuid();
        _parentBId = Guid.NewGuid();

        UsingDbContext(context =>
        {
            context.AcademicYears.Add(new AcademicYear(yearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)));
            context.Grades.Add(new Grade(gradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Classes.Add(new Class(classId, 1, "8A", gradeId, yearId, 30));
            context.FeeStructures.Add(new FeeStructure(structureId, 1, gradeId, yearId, SouthAfricanFeeType.Tuition, "Tuition", 3000m));

            context.Students.Add(new Student(_annId, 1, "Ann", "Adams", new DateTime(2012, 1, 1), Gender.Female, "P001", DateTime.Today, gradeId, classId));
            context.Students.Add(new Student(_benId, 1, "Ben", "Brown", new DateTime(2012, 1, 1), Gender.Male, "P002", DateTime.Today, gradeId, classId));

            context.Parents.Add(new Parent(_parentAId, 1, parentAUser.Id, "Alice", "Adams", "pay.parent.a@test.local", "0710000011"));
            context.Parents.Add(new Parent(_parentBId, 1, parentBUser.Id, "Bob", "Brown", "pay.parent.b@test.local", "0710000012"));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _annId, _parentAId, RelationshipType.Mother));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _benId, _parentBId, RelationshipType.Father));

            var annFee = new StudentFee(Guid.NewGuid(), 1, _annId, structureId, 3000m, DateTime.Today);
            var benFee = new StudentFee(Guid.NewGuid(), 1, _benId, structureId, 3000m, DateTime.Today);
            _benFeeId = benFee.Id;
            context.StudentFees.AddRange(annFee, benFee);

            var annPayment = new Payment(Guid.NewGuid(), 1, _annId, _parentAId, 1000m, SouthAfricanPaymentMethod.EFT, "REC-000001");
            annPayment.MarkAsCompleted();
            var benPayment = new Payment(Guid.NewGuid(), 1, _benId, _parentBId, 1500m, SouthAfricanPaymentMethod.EFT, "REC-000002");
            benPayment.MarkAsCompleted();
            var benPending = new Payment(Guid.NewGuid(), 1, _benId, _parentBId, 1500m, SouthAfricanPaymentMethod.EFT, "REC-000003");
            _annPaymentId = annPayment.Id;
            _benPaymentId = benPayment.Id;
            _benPendingPaymentId = benPending.Id;
            context.Payments.AddRange(annPayment, benPayment, benPending);

            context.PaymentAllocations.Add(new PaymentAllocation(Guid.NewGuid(), annPayment.Id, annFee.Id, 1000m));
            var benAllocation = new PaymentAllocation(Guid.NewGuid(), benPayment.Id, benFee.Id, 1500m);
            _benAllocationId = benAllocation.Id;
            context.PaymentAllocations.Add(benAllocation);
        });
    }

    private User CreateUser(string userName) => UsingDbContext(context =>
    {
        var user = new User
        {
            TenantId = 1,
            UserName = userName,
            Name = userName,
            Surname = "Test",
            EmailAddress = $"{userName}@test.local",
            IsActive = true,
            IsEmailConfirmed = true,
        };
        user.SetNormalizedNames();
        user.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()))
            .HashPassword(user, "Test123!");
        context.Users.Add(user);
        return user;
    });

    private void AssignRole(long userId, string roleName) => UsingDbContext(context =>
    {
        var role = context.Roles.IgnoreQueryFilters().First(r => r.TenantId == 1 && r.Name == roleName);
        context.UserRoles.Add(new UserRole(1, userId, role.Id));
    });

    private void LoginAs(long userId)
    {
        AbpSession.TenantId = 1;
        AbpSession.UserId = userId;
    }

    // ---- reads ----

    [Fact]
    public async Task Parent_can_read_their_own_childs_payment()
    {
        LoginAs(_parentAUserId);

        (await _payments.GetAsync(_annPaymentId)).Id.ShouldBe(_annPaymentId);
        (await _payments.GetByReceiptNumberAsync("REC-000001")).Id.ShouldBe(_annPaymentId);
        (await _payments.GetByStudentAsync(_annId)).Items.Select(p => p.Id).ShouldBe(new[] { _annPaymentId });
        (await _payments.GetByParentAsync(_parentAId)).Items.Select(p => p.Id).ShouldBe(new[] { _annPaymentId });
    }

    [Fact]
    public async Task Parent_cannot_read_another_familys_payment_by_id()
    {
        LoginAs(_parentAUserId);

        var refusal = await Should.ThrowAsync<UserFriendlyException>(() => _payments.GetAsync(_benPaymentId));
        refusal.Message.ShouldBe(FinancialExceptionCodes.PaymentNotFound);
    }

    [Fact]
    public async Task Parent_cannot_walk_the_receipt_numbers()
    {
        LoginAs(_parentAUserId);

        var refusal = await Should.ThrowAsync<UserFriendlyException>(() => _payments.GetByReceiptNumberAsync("REC-000002"));
        refusal.Message.ShouldBe(FinancialExceptionCodes.PaymentNotFound);
    }

    [Fact]
    public async Task Parent_lists_are_empty_for_another_family()
    {
        LoginAs(_parentAUserId);

        (await _payments.GetByStudentAsync(_benId)).Items.ShouldBeEmpty();
        (await _payments.GetByParentAsync(_parentBId)).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Parent_cannot_read_another_familys_allocations()
    {
        LoginAs(_parentAUserId);

        await Should.ThrowAsync<UserFriendlyException>(() => _allocations.GetAsync(_benAllocationId));
        (await _allocations.GetByPaymentAsync(_benPaymentId)).Items.ShouldBeEmpty();
        (await _allocations.GetByStudentFeeAsync(_benFeeId)).Items.ShouldBeEmpty();
        (await _allocations.GetByPaymentAsync(_annPaymentId)).Items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Staff_reads_are_unchanged()
    {
        LoginAs(_financeUserId);

        (await _payments.GetAsync(_benPaymentId)).Id.ShouldBe(_benPaymentId);
        (await _payments.GetByReceiptNumberAsync("REC-000001")).Id.ShouldBe(_annPaymentId);
        (await _payments.GetAllAsync(new GetPaymentsInput { MaxResultCount = 50 })).Items
            .Select(p => p.Id).ShouldBe(new[] { _annPaymentId, _benPaymentId, _benPendingPaymentId }, ignoreOrder: true);
        (await _allocations.GetAsync(_benAllocationId)).Id.ShouldBe(_benAllocationId);
    }

    // ---- settling a payment ----

    [Fact]
    public async Task Parent_cannot_mark_a_payment_as_paid_or_failed()
    {
        LoginAs(_parentAUserId);

        await Should.ThrowAsync<AbpAuthorizationException>(() => _payments.CompleteAsync(_benPendingPaymentId));
        await Should.ThrowAsync<AbpAuthorizationException>(() => _payments.FailAsync(_benPendingPaymentId));

        UsingDbContext(context =>
            context.Payments.Single(p => p.Id == _benPendingPaymentId).Status.ShouldBe(PaymentStatus.Pending));
    }

    [Fact]
    public async Task Staff_can_still_settle_a_payment()
    {
        LoginAs(_financeUserId);

        var settled = await _payments.CompleteAsync(_benPendingPaymentId);

        settled.Status.ShouldBe(PaymentStatus.Completed);
    }
}
