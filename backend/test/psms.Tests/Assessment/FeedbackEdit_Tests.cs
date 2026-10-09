using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Assessment.Marks;
using psms.Assessment.Marks.Dto;
using psms.Assessment.Shared;
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
/// US-TCH-010 / T-T24: feedback is 20–2000 characters, may be edited for 48
/// hours after marks are released, and a refusal for language says which
/// words were the problem — "language that isn't allowed" alone left the
/// teacher guessing what to change.
/// </summary>
public class FeedbackEdit_Tests : psmsTestBase
{
    private const string Good = "Clear working shown throughout; revise fractions before the test.";

    private readonly IMarkAppService _marks;

    public FeedbackEdit_Tests()
    {
        LoginAsDefaultTenantAdmin();
        _marks = Resolve<IMarkAppService>();
    }

    private Guid SeedMark(DateTime? releasedAt = null, string feedback = null)
    {
        var assessmentId = Guid.NewGuid();
        var markId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        UsingDbContext(1, context =>
        {
            // The service reloads the mark with its student, so it needs one.
            context.Students.Add(new Student(studentId, 1, "Ayanda", "Nkosi", new DateTime(2012, 1, 1),
                Gender.Female, $"F{studentId:N}".Substring(0, 8), DateTime.Today, Guid.NewGuid(), Guid.NewGuid()));
            var assessment = new psms.Domain.Assessment.Entities.Assessment(assessmentId, 1, Guid.NewGuid(),
                Guid.NewGuid(), "Term 3 test", AcademicAssessmentType.Test, 50m, 1);
            if (releasedAt.HasValue)
            {
                assessment.MarksReleased = true;
                assessment.MarksReleasedDate = releasedAt;
            }
            context.Assessments.Add(assessment);
            context.Marks.Add(new Mark(markId, 1, assessmentId, studentId) { Feedback = feedback });
            context.SaveChanges();
        });
        return markId;
    }

    private string StoredFeedback(Guid markId)
    {
        string feedback = null;
        UsingDbContext(1, context => feedback = context.Marks.IgnoreQueryFilters().Single(m => m.Id == markId).Feedback);
        return feedback;
    }

    [Fact]
    public async Task Feedback_shorter_than_20_characters_is_refused()
    {
        var markId = SeedMark();

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _marks.UpdateFeedbackAsync(markId, new UpdateFeedbackDto { Feedback = "Good work." }));

        refusal.Message.ShouldBe(AssessmentExceptionCodes.FeedbackTooShort);
    }

    [Fact]
    public async Task The_minimum_counts_trimmed_text()
    {
        var markId = SeedMark();

        await Should.ThrowAsync<UserFriendlyException>(
            () => _marks.UpdateFeedbackAsync(markId, new UpdateFeedbackDto { Feedback = "   Nice.        \n\n          " }));
    }

    [Fact]
    public async Task Feedback_of_20_to_2000_characters_is_saved()
    {
        var markId = SeedMark();

        await _marks.UpdateFeedbackAsync(markId, new UpdateFeedbackDto { Feedback = Good });

        StoredFeedback(markId).ShouldBe(Good);
    }

    [Fact]
    public async Task Clearing_feedback_is_allowed()
    {
        var markId = SeedMark(feedback: Good);

        await _marks.UpdateFeedbackAsync(markId, new UpdateFeedbackDto { Feedback = "" });

        StoredFeedback(markId).ShouldBe(string.Empty);
    }

    [Fact]
    public async Task A_language_refusal_names_the_words()
    {
        var markId = SeedMark();

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _marks.UpdateFeedbackAsync(markId,
                new UpdateFeedbackDto { Feedback = "Lazy effort this term, and the essay was useless." }));

        refusal.Message.ShouldBe(AssessmentExceptionCodes.FeedbackInappropriateLanguage);
        refusal.Details.ShouldContain("\"useless\"");
        refusal.Details.ShouldContain("\"lazy\"");
    }

    [Fact]
    public async Task Feedback_can_be_edited_within_48_hours_of_release()
    {
        var markId = SeedMark(releasedAt: DateTime.UtcNow.AddHours(-47));

        await _marks.UpdateFeedbackAsync(markId, new UpdateFeedbackDto { Feedback = Good });

        StoredFeedback(markId).ShouldBe(Good);
    }

    [Fact]
    public async Task Feedback_is_locked_after_48_hours()
    {
        var markId = SeedMark(releasedAt: DateTime.UtcNow.AddHours(-49));

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _marks.UpdateFeedbackAsync(markId, new UpdateFeedbackDto { Feedback = Good }));

        refusal.Message.ShouldBe(AssessmentExceptionCodes.FeedbackEditWindowExpired);
    }
}
