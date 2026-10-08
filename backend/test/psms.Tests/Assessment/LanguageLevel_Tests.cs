using psms.Domain.Assessment;
using psms.Domain.Shared.Enums;
using Shouldly;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-20. The level a language is offered at — National Protocol §17(6).
/// <para>
/// "In the case of Languages, each language that the learner offers should be
/// recorded and reported on separately according to the different levels on
/// which they are offered. For example, Home Language – English, First
/// Additional Language – IsiXhosa, Second Additional Language – Afrikaans."
/// </para>
/// <para>
/// Not a label: the promotion requirements are written in terms of these levels.
/// NPPPPR §21(1) asks for level 4 "in one language at Home Language level" and
/// level 3 in "the second official language at First Additional Language level";
/// §29(1) asks for 40% in three subjects, "one of which is an official language
/// at Home Language level". A rule that cannot tell the levels apart cannot
/// evaluate either clause.
/// </para>
/// </summary>
public class LanguageLevel_Tests
{
    [Theory]
    [InlineData("English Home Language", LanguageLevel.HomeLanguage)]
    [InlineData("IsiZulu Home Language", LanguageLevel.HomeLanguage)]
    [InlineData("Afrikaans Huistaal", LanguageLevel.HomeLanguage)]
    [InlineData("IsiXhosa First Additional Language", LanguageLevel.FirstAdditionalLanguage)]
    [InlineData("Afrikaans Eerste Addisionele Taal", LanguageLevel.FirstAdditionalLanguage)]
    [InlineData("Sesotho Second Additional Language", LanguageLevel.SecondAdditionalLanguage)]
    [InlineData("Duits Tweede Addisionele Taal", LanguageLevel.SecondAdditionalLanguage)]
    public void A_level_is_read_out_of_the_subjects_name(string name, LanguageLevel expected)
    {
        SubjectLanguageRules.InferFromName(name).ShouldBe(expected);
    }

    [Fact]
    public void Second_additional_is_not_mistaken_for_first_additional()
    {
        // "Second Additional Language" contains "additional", so order matters:
        // test the longer phrase first or every second additional language reads
        // as a first additional one.
        SubjectLanguageRules
            .InferFromName("Sesotho Second Additional Language")
            .ShouldBe(LanguageLevel.SecondAdditionalLanguage);
    }

    [Theory]
    [InlineData("Mathematics")]
    [InlineData("Physical Sciences")]
    [InlineData("Life Orientation")]
    [InlineData("Engineering Graphics and Design")]
    [InlineData("")]
    [InlineData(null)]
    public void A_subject_that_is_not_a_language_has_no_level(string name)
    {
        SubjectLanguageRules.InferFromName(name).ShouldBeNull();
    }

    [Fact]
    public void What_the_school_set_beats_what_the_name_says()
    {
        // A school that corrects a badly named subject should not have the name
        // keep overriding them.
        SubjectLanguageRules
            .ResolveLevel("English", LanguageLevel.HomeLanguage)
            .ShouldBe(LanguageLevel.HomeLanguage);

        SubjectLanguageRules
            .ResolveLevel("English Home Language", LanguageLevel.FirstAdditionalLanguage)
            .ShouldBe(LanguageLevel.FirstAdditionalLanguage);
    }

    [Fact]
    public void An_unset_level_falls_back_to_the_name()
    {
        // Which is what lets this ship without a data migration guessing at
        // every existing subject row.
        SubjectLanguageRules
            .ResolveLevel("IsiZulu First Additional Language", null)
            .ShouldBe(LanguageLevel.FirstAdditionalLanguage);
    }

    /* ─────────── how it reads on the card ─────────── */

    [Fact]
    public void A_name_that_already_says_the_level_is_left_alone()
    {
        // "English Home Language (Home Language)" helps nobody.
        SubjectLanguageRules
            .NameWithLevel("English Home Language", LanguageLevel.HomeLanguage)
            .ShouldBe("English Home Language");

        SubjectLanguageRules
            .NameWithLevel("English Home Language", null)
            .ShouldBe("English Home Language");
    }

    [Fact]
    public void A_name_that_does_not_say_the_level_gets_it_appended()
    {
        SubjectLanguageRules
            .NameWithLevel("English", LanguageLevel.HomeLanguage)
            .ShouldBe("English (Home Language)");
    }

    [Fact]
    public void A_name_whose_level_was_corrected_shows_the_correction()
    {
        // The school says First Additional; the name says Home Language. The
        // school wins, and the card says so rather than quietly disagreeing
        // with itself.
        SubjectLanguageRules
            .NameWithLevel("English Home Language", LanguageLevel.FirstAdditionalLanguage)
            .ShouldBe("English Home Language (First Additional Language)");
    }

    [Fact]
    public void A_subject_that_is_not_a_language_reads_as_it_always_did()
    {
        SubjectLanguageRules.NameWithLevel("Mathematics", null).ShouldBe("Mathematics");
    }

    [Fact]
    public void Each_level_has_the_protocols_own_wording()
    {
        SubjectLanguageRules.LabelFor(LanguageLevel.HomeLanguage).ShouldBe("Home Language");
        SubjectLanguageRules.LabelFor(LanguageLevel.FirstAdditionalLanguage)
            .ShouldBe("First Additional Language");
        SubjectLanguageRules.LabelFor(LanguageLevel.SecondAdditionalLanguage)
            .ShouldBe("Second Additional Language");
        SubjectLanguageRules.LabelFor(null).ShouldBeNull();
    }
}
