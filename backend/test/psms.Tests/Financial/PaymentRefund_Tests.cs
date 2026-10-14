using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.UI;
using psms.Domain.Academic.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Financial.PaymentAllocations;
using psms.Financial.Payments;
using psms.Financial.Shared;
using Shouldly;
using Xunit;

namespace psms.Tests.Financial;

/// <summary>
/// Refunding a payment gives the family the money back, so the fees it paid
/// must show that money as unpaid again. Refunding only flipped the payment's
/// status, so a fully refunded fee still read as Paid.
/// </summary>
public class PaymentRefund_Tests : psmsTestBase
{
    private readonly IPaymentAppService _payments;
    private readonly IPaymentAllocationAppService _allocations;

    private Guid _paymentId;
    private Guid _tuitionId;
    private Guid _levyId;
    private Guid _levyAllocationId;

    public PaymentRefund_Tests()
    {
        _payments = Resolve<IPaymentAppService>();
        _allocations = Resolve<IPaymentAllocationAppService>();
        LoginAsDefaultTenantAdmin();
        Seed();
    }

    /// <summary>
    /// One R2 000 payment: R1 500 settles a R1 500 tuition fee, R500 goes
    /// towards a R1 000 levy.
    /// </summary>
    private void Seed()
    {
        var yearId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var classId = Guid.NewGuid();
        var structureId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        UsingDbContext(context =>
        {
            context.AcademicYears.Add(new AcademicYear(yearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)));
            context.Grades.Add(new Grade(gradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Classes.Add(new Class(classId, 1, "8A", gradeId, yearId, 30));
            context.FeeStructures.Add(new FeeStructure(structureId, 1, gradeId, yearId, SouthAfricanFeeType.Tuition, "Tuition", 1500m));
            context.Students.Add(new Student(studentId, 1, "Ann", "Adams", new DateTime(2012, 1, 1), Gender.Female, "R001", DateTime.Today, gradeId, classId));
            context.Parents.Add(new Parent(parentId, 1, AbpSession.UserId!.Value, "Alice", "Adams", "refund.parent@test.local", "0710000021"));

            var tuition = new StudentFee(Guid.NewGuid(), 1, studentId, structureId, 1500m, DateTime.Today.AddDays(30));
            var levy = new StudentFee(Guid.NewGuid(), 1, studentId, structureId, 1000m, DateTime.Today.AddDays(30));
            tuition.RecordPayment(1500m);
            levy.RecordPayment(500m);
            _tuitionId = tuition.Id;
            _levyId = levy.Id;
            context.StudentFees.AddRange(tuition, levy);

            var payment = new Payment(Guid.NewGuid(), 1, studentId, parentId, 2000m, SouthAfricanPaymentMethod.EFT, "REC-900001");
            payment.MarkAsCompleted();
            _paymentId = payment.Id;
            context.Payments.Add(payment);

            context.PaymentAllocations.Add(new PaymentAllocation(Guid.NewGuid(), payment.Id, tuition.Id, 1500m));
            var levyAllocation = new PaymentAllocation(Guid.NewGuid(), payment.Id, levy.Id, 500m);
            _levyAllocationId = levyAllocation.Id;
            context.PaymentAllocations.Add(levyAllocation);
        });
    }

    private StudentFee Fee(Guid id) => UsingDbContext(context => context.StudentFees.Single(f => f.Id == id));

    [Fact]
    public async Task Refund_takes_the_money_back_off_every_fee_it_paid()
    {
        Fee(_tuitionId).Status.ShouldBe(FeeStatus.Paid);

        await _payments.RefundAsync(_paymentId);

        var tuition = Fee(_tuitionId);
        tuition.AmountPaid.ShouldBe(0m);
        tuition.Status.ShouldBe(FeeStatus.Pending);

        var levy = Fee(_levyId);
        levy.AmountPaid.ShouldBe(0m);
        levy.Status.ShouldBe(FeeStatus.Pending);
    }

    [Fact]
    public async Task Refund_keeps_the_allocations_as_history()
    {
        await _payments.RefundAsync(_paymentId);

        UsingDbContext(context =>
            context.PaymentAllocations.Count(pa => pa.PaymentId == _paymentId).ShouldBe(2));
    }

    [Fact]
    public async Task A_refunded_payments_allocation_cannot_be_removed_and_reversed_twice()
    {
        await _payments.RefundAsync(_paymentId);

        var refusal = await Should.ThrowAsync<UserFriendlyException>(() => _allocations.DeleteAsync(_levyAllocationId));
        refusal.Message.ShouldBe(FinancialExceptionCodes.PaymentNotCompleted);
        Fee(_levyId).AmountPaid.ShouldBe(0m);
    }

    [Fact]
    public async Task A_refunded_waived_fee_stays_waived()
    {
        UsingDbContext(context => context.StudentFees.Single(f => f.Id == _levyId).Waive());

        await _payments.RefundAsync(_paymentId);

        var levy = Fee(_levyId);
        levy.Status.ShouldBe(FeeStatus.Waived);
        levy.AmountPaid.ShouldBe(0m);
    }

    [Fact]
    public async Task Removing_an_allocation_from_a_completed_payment_still_works()
    {
        await _allocations.DeleteAsync(_levyAllocationId);

        var levy = Fee(_levyId);
        levy.AmountPaid.ShouldBe(0m);
        levy.Status.ShouldBe(FeeStatus.Pending);
    }
}
