using Abp.UI;
using Abp.Web.Models;
using psms.Web.Startup;
using Shouldly;
using System;
using Xunit;

namespace psms.Tests.Web;

/// <summary>
/// A refused request is not a server error, and the code is not the message.
/// <para>
/// Every refusal in this codebase is written as
/// <c>new UserFriendlyException(SomeExceptionCodes.Thing, "A sentence.")</c>.
/// ABP's two-argument constructor is <c>(message, details)</c>, so the code
/// lands in the message — the field both clients put in front of a person —
/// and the sentence written for that person lands in the details underneath.
/// And ABP answers every <c>UserFriendlyException</c> with HTTP 500, so a
/// correctly refused request looked exactly like a crashed server.
/// </para>
/// <para>
/// Reading those two conventions in one place beats rewriting nine hundred
/// call sites, but it only works if a code is recognised precisely. These pin
/// that: what counts as one of ours, what must not, and what happens to the
/// sentence.
/// </para>
/// </summary>
public class RefusalsAreNotServerErrors_Tests
{
    /* ─────────── what counts as one of our codes ─────────── */

    [Theory]
    [InlineData("ASM_REPORT_NOT_FOUND")]
    [InlineData("ASM_PDF_NOT_GENERATED")]
    [InlineData("WF_STEP_NOT_FOUND")]
    [InlineData("ACA_CLASS_FULL")]
    [InlineData("ASM_REPORT_CARD_INCOMPLETE")]
    public void A_module_code_is_recognised(string code)
    {
        PsmsErrorCodes.CarriedBy(new UserFriendlyException(code, "A sentence.")).ShouldBe(code);
    }

    [Theory]
    [InlineData("Report not found.")]
    [InlineData("This report card cannot be issued yet.")]
    [InlineData("NOPE")]                      // one shouted word, no underscore
    [InlineData("Not Found")]
    [InlineData("asm_report_not_found")]      // lower case is not our shape
    [InlineData("")]
    public void An_ordinary_message_is_left_alone(string message)
    {
        // The cost of a false positive is a perfectly good message thrown away
        // and replaced by nothing, so the shape is deliberately strict.
        PsmsErrorCodes.CarriedBy(new UserFriendlyException(message)).ShouldBeNull();
    }

    [Fact]
    public void An_exception_that_is_not_a_refusal_carries_no_code()
    {
        PsmsErrorCodes.CarriedBy(new InvalidOperationException("ASM_REPORT_NOT_FOUND")).ShouldBeNull();
        PsmsErrorCodes.CarriedBy(null).ShouldBeNull();
    }

    /* ─────────── which status describes it ─────────── */

    [Theory]
    [InlineData("ASM_REPORT_NOT_FOUND")]
    [InlineData("ASM_REPORT_SUBJECT_NOT_FOUND")]
    [InlineData("WF_INSTANCE_NOT_FOUND")]
    public void Something_that_is_not_there_is_a_not_found(string code)
    {
        PsmsErrorCodes.IsNotFound(code).ShouldBeTrue();
    }

    [Theory]
    [InlineData("ASM_REPORT_CARD_INCOMPLETE")]
    [InlineData("ASM_PDF_NOT_GENERATED")]
    [InlineData("ASM_REPORT_NOT_ISSUED")]
    [InlineData("ASM_NOT_FOUNDATION_PHASE")]
    public void Everything_else_is_not(string code)
    {
        // ASM_PDF_NOT_GENERATED is the trap: the report is there, its PDF has
        // simply not been made yet. "Not generated" is not "not found", and the
        // card is not missing.
        PsmsErrorCodes.IsNotFound(code).ShouldBeFalse();
    }

    /* ─────────── what the caller is shown ─────────── */

    private static ErrorInfo Converted(Exception exception, ErrorInfo asAbpBuiltIt)
    {
        var converter = new PsmsErrorInfoConverter { Next = new Fixed(asAbpBuiltIt) };
        return converter.Convert(exception);
    }

    [Fact]
    public void The_sentence_becomes_the_message_and_the_code_leaves_the_body()
    {
        var error = Converted(
            new UserFriendlyException("ASM_REPORT_CARD_INCOMPLETE", "It still needs the class teacher's comment."),
            new ErrorInfo("ASM_REPORT_CARD_INCOMPLETE", "It still needs the class teacher's comment."));

        error.Message.ShouldBe("It still needs the class teacher's comment.");
        error.Details.ShouldBeNull();
    }

    [Fact]
    public void A_code_with_no_sentence_keeps_the_code()
    {
        // An unhelpful message beats an empty one.
        var error = Converted(
            new UserFriendlyException("ASM_REPORT_NOT_FOUND"),
            new ErrorInfo("ASM_REPORT_NOT_FOUND"));

        error.Message.ShouldBe("ASM_REPORT_NOT_FOUND");
    }

    [Fact]
    public void An_ordinary_error_is_passed_through_untouched()
    {
        var error = Converted(
            new InvalidOperationException("Something broke."),
            new ErrorInfo("An internal error occurred.", "Something broke."));

        error.Message.ShouldBe("An internal error occurred.");
        error.Details.ShouldBe("Something broke.");
    }

    /// <summary>Stands in for the rest of ABP's converter chain.</summary>
    private sealed class Fixed : IExceptionToErrorInfoConverter
    {
        private readonly ErrorInfo _error;

        public Fixed(ErrorInfo error) => _error = error;

        public IExceptionToErrorInfoConverter Next { private get; set; }

        public ErrorInfo Convert(Exception exception) => _error;
    }
}
