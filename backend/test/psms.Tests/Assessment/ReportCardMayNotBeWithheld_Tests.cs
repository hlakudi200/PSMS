using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Shouldly;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RE-004. A report card may not be withheld from a learner for any reason.
/// <para>
/// National Protocol §25(13): "Schools may **not withhold report cards from
/// learners for any reason whatsoever**", and §25(12) gives parents a right of
/// access. Withholding a card over unpaid fees is common practice and is
/// explicitly not permitted — a system that offers it as a feature is offering
/// a school a way to break policy.
/// </para>
/// <para>
/// There is no clean unit test for "nobody ever added a fees check", so this
/// reads the service instead. It is a guard against the feature being added by
/// someone who has not read §25(13), which is exactly how it would arrive.
/// </para>
/// </summary>
public class ReportCardMayNotBeWithheld_Tests
{
    private static string ReportServiceSource()
    {
        // Walk up from the test binary to the repository, then to the service.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "psms.Application")))
            dir = dir.Parent;

        dir.ShouldNotBeNull("could not find the backend source from the test binary");

        var path = Path.Combine(dir.FullName, "src", "psms.Application",
            "Assessment", "Reports", "ReportAppService.cs");

        File.Exists(path).ShouldBeTrue($"expected the report service at {path}");

        return File.ReadAllText(path);
    }

    [Theory]
    [InlineData("fee")]
    [InlineData("outstanding balance")]
    [InlineData("arrears")]
    [InlineData("debtor")]
    public void Nothing_on_a_report_card_path_looks_at_what_the_family_owes(string term)
    {
        var source = ReportServiceSource();

        // Comments are allowed to mention fees — RE-004 is written down in
        // them. Code is not.
        var code = string.Join('\n', source
            .Split('\n')
            .Where(line => !line.TrimStart().StartsWith("//")
                        && !line.TrimStart().StartsWith("///")
                        && !line.TrimStart().StartsWith("*")));

        code.IndexOf(term, StringComparison.OrdinalIgnoreCase)
            .ShouldBe(-1,
                $"§25(13): a report card may not be withheld for any reason, and '{term}' "
                + "appearing in the report service is how that rule gets broken");
    }

    [Fact]
    public void The_rule_is_written_down_where_the_next_person_will_look()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "docs")))
            dir = dir.Parent;

        dir.ShouldNotBeNull("could not find backend/docs from the test binary");

        var rules = File.ReadAllText(Path.Combine(dir.FullName, "docs", "PSMS-Business-Rules.md"));

        rules.ShouldContain("RE-004");
        rules.ShouldContain("not withhold report cards");
        rules.ShouldContain("RE-005");
        rules.ShouldContain("§25(3)");
    }
}
