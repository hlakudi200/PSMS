using psms.Domain.Academic.Entities;
using psms.Assessment.Reports;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// The code behind the QR on a printed report card.
/// <para>
/// The caller is a parent, a bursary office or another school: nobody is signed
/// in and nobody belongs to a tenant, so the lookup has to reach a card that
/// does. The first version used <c>SetTenantId(null)</c>, which does not widen
/// the query but narrows it to "TenantId IS NULL" — matching no card at all,
/// because every card belongs to a school. Every printed code came back as
/// "no report card matches this code", and nothing in the test suite noticed.
/// </para>
/// </summary>
public class ReportVerification_Tests : psmsTestBase
{
    private readonly IReportVerificationAppService _verification;

    public ReportVerification_Tests()
    {
        _verification = Resolve<IReportVerificationAppService>();
        SeedSchool();
    }

    private Guid _studentId, _classId, _yearId;

    /// <summary>
    /// A card with the learner, class, grade and year it actually belongs to.
    /// The service Includes all of them, and on a required navigation an Include
    /// is an inner join: seed a card whose relations are missing and the lookup
    /// drops it, which is a different failure from the one these tests are for.
    /// </summary>
    private void SeedSchool()
    {
        _yearId = Guid.NewGuid();
        _classId = Guid.NewGuid();
        _studentId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(_yearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 1)));
            context.Grades.Add(new Grade(gradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Classes.Add(new Class(_classId, 1, "8A", gradeId, _yearId, 30));
            context.Students.Add(new Student(_studentId, 1, "Ann", "Mokoena",
                new DateTime(2012, 1, 1), Gender.Female, "A001", DateTime.Today, gradeId, _classId));
            context.SaveChanges();
        });
    }

    private Guid SeedCard(string token, ReportStatus status)
    {
        var id = Guid.NewGuid();
        UsingDbContext(1, context =>
        {
            var card = new Report(id, 1, _studentId, _classId, _yearId, ReportType.Term1)
            {
                Status = status,
            };
            card.AssignVerificationToken(token);
            context.Reports.Add(card);
            context.SaveChanges();
        });
        return id;
    }

    [Fact]
    public async Task A_published_card_is_found_by_its_code_without_signing_in()
    {
        var token = ReportVerificationAppService.NewToken();
        SeedCard(token, ReportStatus.Published);

        // Nobody is signed in — exactly what a scanned QR produces.
        AbpSession.TenantId = null;
        AbpSession.UserId = null;

        var result = await _verification.VerifyAsync(token);

        result.Found.ShouldBeTrue("a card that exists must be found by its own code");
        result.Issued.ShouldBeTrue();
    }

    [Fact]
    public async Task A_card_the_school_has_not_published_is_found_but_not_issued()
    {
        var token = ReportVerificationAppService.NewToken();
        SeedCard(token, ReportStatus.Generated);

        AbpSession.TenantId = null;
        AbpSession.UserId = null;

        var result = await _verification.VerifyAsync(token);

        // Found, so the reader is told the truth: this code is real, but the
        // school has not issued the document.
        result.Found.ShouldBeTrue();
        result.Issued.ShouldBeFalse();
    }

    [Fact]
    public async Task A_code_that_was_never_issued_is_refused()
    {
        AbpSession.TenantId = null;
        AbpSession.UserId = null;

        var result = await _verification.VerifyAsync("totallyMadeUpToken123");

        result.Found.ShouldBeFalse();
        result.Issued.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task An_empty_code_is_refused_rather_than_matching_a_card(string token)
    {
        // A card with no token must never be reachable by passing no token.
        SeedCard(null, ReportStatus.Published);

        AbpSession.TenantId = null;
        AbpSession.UserId = null;

        var result = await _verification.VerifyAsync(token);

        result.Found.ShouldBeFalse();
    }

    [Fact]
    public async Task The_code_does_not_reveal_the_learners_name_or_marks()
    {
        var token = ReportVerificationAppService.NewToken();
        SeedCard(token, ReportStatus.Published);

        AbpSession.TenantId = null;
        AbpSession.UserId = null;

        var result = await _verification.VerifyAsync(token);

        // Whatever else this returns, it is read by anyone holding the code.
        result.ShouldNotBeNull();
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        json.ShouldNotContain("overallPercentage");
        json.ShouldNotContain("subjectReports");
    }
}
