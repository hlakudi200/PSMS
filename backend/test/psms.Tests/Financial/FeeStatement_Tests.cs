using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Domain.Academic.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using psms.Financial.Shared;
using psms.Financial.Statements;
using psms.Financial.Statements.Dto;
using psms.Tests.Assessment;
using Shouldly;
using Xunit;

namespace psms.Tests.Financial;

/// <summary>
/// MOB-BE-01 (#215): a parent can read their own children's fee statement —
/// one child or the whole family — and nobody else's. Staff name the learner.
/// <para>
/// Family A is a parent with two children (Ann and Amy); family B a parent
/// with one (Ben). Ann has paid R1 000 of R3 000; Amy owes R2 000; Ben R1 500.
/// </para>
/// </summary>
[Collection(PdfRenderCollection.Name)]
public class FeeStatement_Tests : psmsTestBase
{
    private readonly IFeeStatementAppService _statements;

    private Guid _annId;
    private Guid _amyId;
    private Guid _benId;
    private Guid _parentAId;
    private long _parentAUserId;
    private long _parentBUserId;
    private long _financeUserId;

    private static readonly DateTime From = DateTime.Today.AddDays(-30);
    private static readonly DateTime To = DateTime.Today;

    public FeeStatement_Tests()
    {
        _statements = Resolve<IFeeStatementAppService>();
        Seed();
    }

    private void Seed()
    {
        var parentAUser = CreateUser("fees.parent.a");
        var parentBUser = CreateUser("fees.parent.b");
        var financeUser = CreateUser("fees.finance");
        _parentAUserId = parentAUser.Id;
        _parentBUserId = parentBUser.Id;
        _financeUserId = financeUser.Id;

        AssignRole(parentAUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(parentBUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(financeUser.Id, StaticRoleNames.Tenants.Finance);

        var yearId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var classId = Guid.NewGuid();
        var structureId = Guid.NewGuid();
        _annId = Guid.NewGuid();
        _amyId = Guid.NewGuid();
        _benId = Guid.NewGuid();
        _parentAId = Guid.NewGuid();
        var parentBId = Guid.NewGuid();

        UsingDbContext(context =>
        {
            context.AcademicYears.Add(new AcademicYear(yearId, 1, DateTime.Today.Year,
                new DateTime(DateTime.Today.Year, 1, 1), new DateTime(DateTime.Today.Year, 12, 31)));
            context.Grades.Add(new Grade(gradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Classes.Add(new Class(classId, 1, "8A", gradeId, yearId, 30));
            context.FeeStructures.Add(new FeeStructure(structureId, 1, gradeId, yearId, SouthAfricanFeeType.Tuition, "Tuition", 3000m));

            context.Students.Add(new Student(_annId, 1, "Ann", "Adams", new DateTime(2012, 1, 1), Gender.Female, "F001", DateTime.Today, gradeId, classId));
            context.Students.Add(new Student(_amyId, 1, "Amy", "Adams", new DateTime(2014, 1, 1), Gender.Female, "F002", DateTime.Today, gradeId, classId));
            context.Students.Add(new Student(_benId, 1, "Ben", "Brown", new DateTime(2012, 1, 1), Gender.Male, "F003", DateTime.Today, gradeId, classId));

            context.Parents.Add(new Parent(_parentAId, 1, parentAUser.Id, "Alice", "Adams", "fees.parent.a@test.local", "0710000001"));
            context.Parents.Add(new Parent(parentBId, 1, parentBUser.Id, "Bob", "Brown", "fees.parent.b@test.local", "0710000002"));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _annId, _parentAId, RelationshipType.Mother));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _amyId, _parentAId, RelationshipType.Mother));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _benId, parentBId, RelationshipType.Father));

            var annFee = new StudentFee(Guid.NewGuid(), 1, _annId, structureId, 3000m, DateTime.Today.AddDays(-10));
            context.StudentFees.Add(annFee);
            context.StudentFees.Add(new StudentFee(Guid.NewGuid(), 1, _amyId, structureId, 2000m, DateTime.Today.AddDays(10)));
            context.StudentFees.Add(new StudentFee(Guid.NewGuid(), 1, _benId, structureId, 1500m, DateTime.Today.AddDays(-10)));

            var payment = new Payment(Guid.NewGuid(), 1, _annId, _parentAId, 1000m, SouthAfricanPaymentMethod.EFT, "RCPT-001");
            payment.MarkAsCompleted();
            context.Payments.Add(payment);
            context.PaymentAllocations.Add(new PaymentAllocation(Guid.NewGuid(), payment.Id, annFee.Id, 1000m));
            annFee.RecordPayment(1000m);

            // Never reached the school: must not appear.
            context.Payments.Add(new Payment(Guid.NewGuid(), 1, _annId, _parentAId, 500m, SouthAfricanPaymentMethod.EFT, "RCPT-002"));
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

    private static GetFeeStatementInput LastMonth(Guid? studentId = null) =>
        new() { StudentId = studentId, FromDate = From, ToDate = To };

    [Fact]
    public async Task Parent_gets_one_statement_for_all_their_children_and_no_one_else()
    {
        LoginAs(_parentAUserId);

        var statement = await _statements.GetStatementAsync(LastMonth());

        statement.Children.Select(c => c.StudentId).ShouldBe(new[] { _amyId, _annId }, ignoreOrder: true);
        statement.TotalCharged.ShouldBe(5000m);
        statement.TotalPaid.ShouldBe(1000m);
        statement.ClosingBalance.ShouldBe(4000m);
        // Ann's fee is past due; Amy's is not due yet.
        statement.AmountOverdue.ShouldBe(2000m);
        statement.Lines.ShouldNotContain(l => l.Reference == "RCPT-002");
        statement.Lines.ShouldNotContain(l => l.StudentId == _benId);
    }

    [Fact]
    public async Task Parent_can_ask_for_one_child()
    {
        LoginAs(_parentAUserId);

        var statement = await _statements.GetStatementAsync(LastMonth(_amyId));

        statement.Children.Single().StudentId.ShouldBe(_amyId);
        statement.ClosingBalance.ShouldBe(2000m);
        statement.Children.Single().OutstandingFees.Single().Outstanding.ShouldBe(2000m);
    }

    [Fact]
    public async Task Parent_cannot_read_another_familys_statement()
    {
        LoginAs(_parentAUserId);

        var ex = await Should.ThrowAsync<UserFriendlyException>(() => _statements.GetStatementAsync(LastMonth(_benId)));
        ex.Message.ShouldBe(FinancialExceptionCodes.StudentNotFound);

        await Should.ThrowAsync<UserFriendlyException>(() => _statements.GenerateStatementPdfAsync(LastMonth(_benId)));
    }

    [Fact]
    public async Task Other_parent_sees_only_their_own_child()
    {
        LoginAs(_parentBUserId);

        var statement = await _statements.GetStatementAsync(LastMonth());

        statement.Children.Select(c => c.StudentId).ShouldBe(new[] { _benId });
        statement.ClosingBalance.ShouldBe(1500m);
    }

    [Fact]
    public async Task Staff_must_name_the_learner()
    {
        LoginAs(_financeUserId);

        var ex = await Should.ThrowAsync<UserFriendlyException>(() => _statements.GetStatementAsync(LastMonth()));
        ex.Message.ShouldBe(FinancialExceptionCodes.StatementStudentRequired);

        var statement = await _statements.GetStatementAsync(LastMonth(_benId));
        statement.ClosingBalance.ShouldBe(1500m);
    }

    [Fact]
    public async Task Only_one_kind_of_period_can_be_chosen()
    {
        LoginAs(_parentAUserId);

        var input = LastMonth();
        input.AcademicYearId = Guid.NewGuid();

        var ex = await Should.ThrowAsync<UserFriendlyException>(() => _statements.GetStatementAsync(input));
        ex.Message.ShouldBe(FinancialExceptionCodes.StatementPeriodInvalid);

        var backwards = new GetFeeStatementInput { FromDate = To, ToDate = From };
        (await Should.ThrowAsync<UserFriendlyException>(() => _statements.GetStatementAsync(backwards)))
            .Message.ShouldBe(FinancialExceptionCodes.StatementPeriodInvalid);
    }

    [Fact]
    public async Task Periods_list_the_schools_academic_years()
    {
        LoginAs(_parentAUserId);

        var periods = await _statements.GetPeriodsAsync();

        periods.ShouldContain(y => y.YearName == $"{DateTime.Today.Year} Academic Year");
    }

    [Fact]
    public async Task Generated_pdf_is_stored_privately_under_the_parent_and_says_what_is_owed()
    {
        var storage = Resolve<IFileStorageService>();
        byte[] uploaded = null;
        string uploadedPath = null;
        storage.DefaultBucketName.Returns("documents");
        storage.UploadAsync(Arg.Any<string>(), Arg.Do<byte[]>(b => uploaded = b), "application/pdf")
            .Returns(ci => { uploadedPath = ci.ArgAt<string>(0); return uploadedPath; });
        storage.CreateSignedDownloadUrlAsync("documents", Arg.Any<string>(), 300)
            .Returns("https://storage.test/signed");

        LoginAs(_parentAUserId);

        var url = await _statements.GenerateStatementPdfAsync(LastMonth());

        url.ShouldBe("https://storage.test/signed");
        uploadedPath.ShouldStartWith($"statements/1/parent-{_parentAId}/family/");
        await storage.Received(1).CreateSignedDownloadUrlAsync("documents", uploadedPath, 300);

        var text = PdfText.Extract(uploaded);
        text.ShouldContain("Fee Statement");
        text.ShouldContain("Alice Adams");
        text.ShouldContain("Ann Adams");
        text.ShouldContain("Amy Adams");
        text.ShouldContain("4,000.00");
        text.ShouldContain("RCPT-001");
        text.ShouldNotContain("Ben Brown");
    }
}
