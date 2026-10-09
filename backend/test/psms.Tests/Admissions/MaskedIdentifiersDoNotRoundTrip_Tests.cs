using psms.Shared;
using Shouldly;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A mask is something to show, never something to store.
/// <para>
/// ID and passport numbers go out masked — <c>*********5081</c>. A parent who
/// saved a draft and came back found that mask sitting in the ID field, and
/// Save and continue failed the thirteen-digit check on a number they had
/// typed correctly. They could not get past it; the field refilled itself with
/// the mask on every visit.
/// </para>
/// <para>
/// The quieter half: had that check not fired, the asterisks would have been
/// written over the real identifier. Passport numbers have no checksum and no
/// length rule, so nothing was stopping that at all.
/// </para>
/// </summary>
public class MaskedIdentifiersDoNotRoundTrip_Tests
{
    [Theory]
    [InlineData("1004020500089")]
    [InlineData("A12345678")]
    [InlineData("AB12")]
    [InlineData("X1")]
    public void Anything_this_class_masks_is_recognisable_as_masked(string real)
    {
        PiiMasking.LooksMasked(PiiMasking.Mask(real)).ShouldBeTrue();
    }

    [Theory]
    [InlineData("1004020500089")]
    [InlineData("9501015800085")]
    [InlineData("A12345678")]
    [InlineData("EA1234567")]
    public void And_a_real_identifier_never_is(string real)
    {
        PiiMasking.LooksMasked(real).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Nothing_is_not_a_mask(string value)
    {
        // An empty field means "not given", which an update must be free to
        // treat on its own terms rather than mistaking for a masked read.
        PiiMasking.LooksMasked(value).ShouldBeFalse();
    }

    [Fact]
    public void The_last_four_digits_are_what_identifies_it_on_screen()
    {
        PiiMasking.Mask("1004020500089").ShouldBe("*********0089");
    }

    [Fact]
    public void A_short_value_gives_nothing_away()
    {
        PiiMasking.Mask("AB12").ShouldBe("****");
    }
}
