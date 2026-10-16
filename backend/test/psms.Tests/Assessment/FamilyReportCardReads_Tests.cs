using Abp.Authorization.Users;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using psms.Assessment.Reports;
using psms.Assessment.Reports.Dto;
using psms.Assessment.Shared;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// What a family reads of a report card, and how often they can acknowledge it.
/// <para>
/// RC-19: a Foundation Phase card (Grades R–3) reaches a parent or learner with
/// no percentage and no rank on any read, as on the PDF they are given. Staff
/// still see the figures, and a Grade 8 card is unchanged for everyone.
/// </para>
/// <para>
/// RE-004: GetByStudentTerm, like Get, shows a family only an issued card.
/// Promotion advice is staff only (#444).
/// </para>
/// <para>
/// A parent acknowledges a card once. A second call no longer overwrites the
/// date and comment, and a card the school withdraws and issues again starts
/// unacknowledged.
/// </para>
/// </summary>
public class FamilyReportCardReads_Tests : psmsTestBase
{
    private readonly IReportAppService _reports;

    private Guid _foundationStudentId;
    private Guid _seniorStudentId;
    private Guid _foundationReportId;
    private Guid _seniorReportId;
    private Guid _draftReportId;
    private Guid _termId;
    private long _foundationStudentUserId;
    private long _parentUserId;

    public FamilyReportCardReads_Tests()
    {
        _reports = Resolve<IReportAppService>();
        Seed();
    }

    private void Seed()
    {
        var foundationStudentUser = CreateUser("rc.foundation.student");
        var seniorStudentUser = CreateUser("rc.senior.student");
        var parentUser = CreateUser("rc.parent");
        _foundationStudentUserId = foundationStudentUser.Id;
        _parentUserId = parentUser.Id;
        AssignRole(foundationStudentUser.Id, StaticRoleNames.Tenants.Student);
        AssignRole(seniorStudentUser.Id, StaticRoleNames.Tenants.Student);
        AssignRole(parentUser.Id, StaticRoleNames.Tenants.Parent);

        var academicYearId = Guid.NewGuid();
        var foundationGradeId = Guid.NewGuid();
        var seniorGradeId = Guid.NewGuid();
        var foundationClassId = Guid.NewGuid();
        var seniorClassId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        _termId = Guid.NewGuid();
        _foundationStudentId = Guid.NewGuid();
        _seniorStudentId = Guid.NewGuid();
        _foundationReportId = Guid.NewGuid();
        _seniorReportId = Guid.NewGuid();
        _draftReportId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(academicYearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 1)));
            context.Terms.Add(new Term(_termId, 1, academicYearId, SouthAfricanTermNumber.Term3, new DateTime(2026, 7, 14), new DateTime(2026, 9, 26)));
            context.Grades.Add(new Grade(foundationGradeId, 1, SouthAfricanGradeLevel.Grade3, "Grade 3", SouthAfricanSchoolPhase.Foundation));
            context.Grades.Add(new Grade(seniorGradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Classes.Add(new Class(foundationClassId, 1, "3A", foundationGradeId, academicYearId, 30));
            context.Classes.Add(new Class(seniorClassId, 1, "8A", seniorGradeId, academicYearId, 30));
            context.Subjects.Add(new Subject(subjectId, 1, "Mathematics", "MATH", true));

            context.Students.Add(new Student(_foundationStudentId, 1, "Ayanda", "Nkosi", new DateTime(2018, 1, 1),
                Gender.Female, "F0001", DateTime.Today, foundationGradeId, foundationClassId) { UserId = foundationStudentUser.Id });
            context.Students.Add(new Student(_seniorStudentId, 1, "Sipho", "Nkosi", new DateTime(2013, 1, 1),
                Gender.Male, "S0001", DateTime.Today, seniorGradeId, seniorClassId) { UserId = seniorStudentUser.Id });

            // One parent of both learners, so the same caller reads both phases.
            context.Parents.Add(new Parent(parentId, 1, parentUser.Id, "Parent", "Nkosi", "rc.parent@test.local", "0710000000"));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _foundationStudentId, parentId, RelationshipType.Guardian));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _seniorStudentId, parentId, RelationshipType.Guardian));

            context.Reports.Add(MarkedCard(_foundationReportId, _foundationStudentId, foundationClassId, academicYearId, subjectId, ReportType.Term3, ReportStatus.Published));
            context.Reports.Add(MarkedCard(_seniorReportId, _seniorStudentId, seniorClassId, academicYearId, subjectId, ReportType.Term3, ReportStatus.Published));
            // The same learner's mid-year card, still being approved.
            context.Reports.Add(MarkedCard(_draftReportId, _foundationStudentId, foundationClassId, academicYearId, subjectId, ReportType.MidYear, ReportStatus.PendingApproval));
            context.SaveChanges();
        });
    }

    private Report MarkedCard(Guid id, Guid studentId, Guid classId, Guid academicYearId, Guid subjectId,
        ReportType type, ReportStatus status)
    {
        var report = new Report(id, 1, studentId, classId, academicYearId, type, _termId);
        var row = new ReportSubject(Guid.NewGuid(), id, subjectId)
        {
            TermMark = 70m,
            ExamMark = 80m,
            FinalMark = 76m,
            AchievementLevel = CapsAchievementLevel.Level6,
            SubjectPosition = 3,
            ClassAverage = 61.5m,
            HighestInClass = 92m,
            LowestInClass = 31m,
        };
        report.SubjectReports.Add(row);
        report.OverallPercentage = 76m;
        report.OverallAchievementLevel = CapsAchievementLevel.Level6;
        report.SetClassPosition(3);
        report.SetCohortSize(30);
        report.Status = status;
        return report;
    }

    private User CreateUser(string userName)
    {
        return UsingDbContext(context =>
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
    }

    private void AssignRole(long userId, string roleName)
    {
        UsingDbContext(context =>
        {
            var role = context.Roles.IgnoreQueryFilters().First(r => r.TenantId == 1 && r.Name == roleName);
            context.UserRoles.Add(new UserRole(1, userId, role.Id));
        });
    }

    private void LoginAs(long userId)
    {
        AbpSession.TenantId = 1;
        AbpSession.UserId = userId;
    }

    private static void ShouldCarryNoPercentage(ReportDto card)
    {
        card.ReportsPercentages.ShouldBeFalse();
        card.OverallPercentage.ShouldBeNull();
        card.ClassPosition.ShouldBeNull();
        card.TotalStudentsInClass.ShouldBeNull();
        // The level is what the phase reports, so it stays.
        card.OverallAchievementLevel.ShouldBe(CapsAchievementLevel.Level6);

        var subject = card.SubjectReports.ShouldHaveSingleItem();
        subject.TermMark.ShouldBeNull();
        subject.ExamMark.ShouldBeNull();
        subject.FinalMark.ShouldBeNull();
        subject.SubjectPosition.ShouldBeNull();
        subject.ClassAverage.ShouldBeNull();
        subject.HighestInClass.ShouldBeNull();
        subject.LowestInClass.ShouldBeNull();
        subject.AchievementLevel.ShouldBe(CapsAchievementLevel.Level6);
    }

    private static void ShouldCarryPercentages(ReportDto card)
    {
        card.OverallPercentage.ShouldBe(76m);
        card.ClassPosition.ShouldBe(3);
        card.TotalStudentsInClass.ShouldBe(30);

        var subject = card.SubjectReports.ShouldHaveSingleItem();
        subject.FinalMark.ShouldBe(76m);
        subject.ClassAverage.ShouldBe(61.5m);
        subject.SubjectPosition.ShouldBe(3);
    }

    // ─── RC-19: Foundation Phase figures ───

    [Fact]
    public async Task A_parent_reads_a_foundation_phase_card_without_percentages()
    {
        LoginAs(_parentUserId);

        ShouldCarryNoPercentage(await _reports.GetAsync(_foundationReportId));
    }

    [Fact]
    public async Task A_learner_reads_their_own_foundation_phase_card_without_percentages()
    {
        LoginAs(_foundationStudentUserId);

        ShouldCarryNoPercentage(await _reports.GetAsync(_foundationReportId));
    }

    [Fact]
    public async Task The_by_term_read_withholds_them_too()
    {
        LoginAs(_parentUserId);

        ShouldCarryNoPercentage(await _reports.GetByStudentTermAsync(_foundationStudentId, _termId, ReportType.Term3));
    }

    [Fact]
    public async Task The_list_withholds_the_overall_and_the_position()
    {
        LoginAs(_parentUserId);

        var rows = (await _reports.GetAllAsync(new GetReportsInput { MaxResultCount = 10 })).Items;

        var foundation = rows.Single(r => r.Id == _foundationReportId);
        foundation.ReportsPercentages.ShouldBeFalse();
        foundation.OverallPercentage.ShouldBeNull();
        foundation.ClassPosition.ShouldBeNull();
        foundation.OverallAchievementLevel.ShouldBe(CapsAchievementLevel.Level6);

        var senior = rows.Single(r => r.Id == _seniorReportId);
        senior.OverallPercentage.ShouldBe(76m);
        senior.ClassPosition.ShouldBe(3);
    }

    [Fact]
    public async Task A_senior_phase_card_still_carries_its_percentages_for_a_parent()
    {
        LoginAs(_parentUserId);

        ShouldCarryPercentages(await _reports.GetAsync(_seniorReportId));
    }

    [Fact]
    public async Task Staff_still_see_a_foundation_phase_cards_figures()
    {
        LoginAsDefaultTenantAdmin();

        var card = await _reports.GetAsync(_foundationReportId);

        card.ReportsPercentages.ShouldBeFalse();
        ShouldCarryPercentages(card);
    }

    // ─── RE-004: the by-term read ───

    [Fact]
    public async Task A_parent_cannot_read_an_unissued_card_by_term()
    {
        LoginAs(_parentUserId);

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _reports.GetByStudentTermAsync(_foundationStudentId, _termId, ReportType.MidYear));
        refusal.Message.ShouldBe(AssessmentExceptionCodes.ReportNotFound);
    }

    [Fact]
    public async Task Staff_can_still_read_it_by_term()
    {
        LoginAsDefaultTenantAdmin();

        var card = await _reports.GetByStudentTermAsync(_foundationStudentId, _termId, ReportType.MidYear);

        card.Id.ShouldBe(_draftReportId);
    }

    // ─── #444: promotion advice is staff only ───

    [Fact]
    public async Task A_parent_cannot_read_promotion_advice_even_for_their_own_issued_card()
    {
        LoginAs(_parentUserId);

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _reports.GetPromotionAdviceAsync(_seniorReportId));
        refusal.Message.ShouldBe(AssessmentExceptionCodes.ReportNotFound);
    }

    [Fact]
    public async Task A_learner_cannot_read_promotion_advice_for_their_own_unissued_card()
    {
        LoginAs(_foundationStudentUserId);

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _reports.GetPromotionAdviceAsync(_draftReportId));
        refusal.Message.ShouldBe(AssessmentExceptionCodes.ReportNotFound);
    }

    [Fact]
    public async Task Staff_still_read_promotion_advice()
    {
        LoginAsDefaultTenantAdmin();

        var advice = await _reports.GetPromotionAdviceAsync(_seniorReportId);

        advice.ReportId.ShouldBe(_seniorReportId);
    }

    // ─── Acknowledgement, once ───

    [Fact]
    public async Task A_parent_acknowledges_a_card_once()
    {
        LoginAs(_parentUserId);

        var first = await _reports.AcknowledgeByParentAsync(_seniorReportId, new ParentAcknowledgementDto { Comment = "Thank you." });
        first.ParentAcknowledgedDate.ShouldNotBeNull();

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _reports.AcknowledgeByParentAsync(_seniorReportId, new ParentAcknowledgementDto { Comment = "Again." }));
        refusal.Message.ShouldBe(AssessmentExceptionCodes.ReportAlreadyAcknowledged);

        var kept = UsingDbContext(1, context => context.Reports.Single(r => r.Id == _seniorReportId));
        kept.ParentAcknowledgedDate.ShouldBe(first.ParentAcknowledgedDate);
        kept.ParentComment.ShouldBe("Thank you.");
    }

    [Fact]
    public void A_card_issued_again_after_a_withdrawal_starts_unacknowledged()
    {
        var report = new Report(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ReportType.Term3)
        {
            Status = ReportStatus.Published,
        };
        report.AcknowledgeByParent("Seen.");

        report.Withdraw(1, "A mark was captured against the wrong learner.");
        report.Publish();

        report.ParentAcknowledgedDate.ShouldBeNull();
        report.ParentComment.ShouldBeNull();
    }
}
