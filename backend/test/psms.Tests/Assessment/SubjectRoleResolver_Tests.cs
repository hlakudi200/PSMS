using psms.Domain.Assessment.Promotion;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-16. Reading a subject's part in the promotion requirements off its name.
/// <para>
/// This is the one guess in the promotion rules — the data model carries no
/// language level (RC-20) — so it is worth pinning hard. A misresolution here
/// does not produce a wrong-looking mark; it produces a wrong promotion.
/// </para>
/// </summary>
public class SubjectRoleResolver_Tests
{
    [Theory]
    [InlineData("English Home Language", "ENGHL")]
    [InlineData("English Home Language", null)]
    [InlineData("isiXhosa Home Language", "")]
    [InlineData("Afrikaans Huistaal", null)]
    // Written the short way, which the resolver used to look for only in the
    // code — so "English (HL)" resolved to nothing, the Home Language clause
    // reported "no subject in that role", and the learner was advised retained.
    [InlineData("English (HL)", null)]
    [InlineData("English HL", null)]
    [InlineData("Sesotho", "SESHL")]
    public void A_home_language_is_recognised_however_it_is_written(string name, string code)
    {
        Assert.Equal(SubjectRole.HomeLanguage, SubjectRoleResolver.Resolve(name, code));
    }

    [Theory]
    [InlineData("isiZulu First Additional Language", null)]
    [InlineData("Afrikaans Eerste Addisionele Taal", null)]
    [InlineData("isiZulu (FAL)", null)]
    [InlineData("Anything", "ZULFAL")]
    public void A_first_additional_language_likewise(string name, string code)
    {
        Assert.Equal(SubjectRole.FirstAdditionalLanguage, SubjectRoleResolver.Resolve(name, code));
    }

    [Theory]
    [InlineData("German Second Additional Language", null)]
    [InlineData("Anything", "GERSAL")]
    public void And_a_second_additional_language(string name, string code)
    {
        Assert.Equal(SubjectRole.SecondAdditionalLanguage, SubjectRoleResolver.Resolve(name, code));
    }

    [Theory]
    [InlineData("Mathematics", "MATH")]
    [InlineData("Mathematics", null)]
    // The match used to be exact equality, so every one of these fell through to
    // Other: the Mathematics clause reported "no subject in that role" and the
    // learner was advised retained, while the maths mark quietly satisfied one
    // of the "any three of the others" clauses instead.
    [InlineData("Maths", null)]
    [InlineData("Mathematics Grade 8", null)]
    [InlineData("Core Mathematics", null)]
    [InlineData("Wiskunde", null)]
    public void Mathematics_is_recognised_however_it_is_written(string name, string code)
    {
        Assert.Equal(SubjectRole.Mathematics, SubjectRoleResolver.Resolve(name, code));
    }

    [Fact]
    public void A_maths_code_ending_in_HL_is_still_mathematics()
    {
        // "MATHL" — Maths Higher Level. The code suffix used to be tested first,
        // so this resolved as a Home Language: the maths mark satisfied the
        // language clause and Mathematics went unrepresented.
        Assert.Equal(SubjectRole.Mathematics, SubjectRoleResolver.Resolve("Mathematics", "MATHL"));
    }

    [Fact]
    public void Mathematical_literacy_is_not_mathematics()
    {
        // A different subject, offered in place of Mathematics in the FET phase
        // — where §29(1) is stated in percentages over all subjects and never
        // singles Mathematics out.
        Assert.Equal(SubjectRole.Other,
            SubjectRoleResolver.Resolve("Mathematical Literacy", "MLIT"));
    }

    [Theory]
    [InlineData("Life Sciences", "LSCI")]
    [InlineData("Physical Sciences", "PHSC")]
    [InlineData("Life Orientation", "LO")]
    [InlineData("Highland Studies", null)]   // contains "hl", but not as a word
    [InlineData("Salesmanship", null)]       // contains "sal", but not as a word
    [InlineData(null, null)]
    [InlineData("", "")]
    public void Everything_else_is_an_ordinary_required_subject(string name, string code)
    {
        Assert.Equal(SubjectRole.Other, SubjectRoleResolver.Resolve(name, code));
    }
}
