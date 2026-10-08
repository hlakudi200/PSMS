using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// Every test that renders a report card runs in this collection, which means
/// one at a time.
/// <para>
/// xUnit runs test classes in parallel, and QuestPDF keeps process-wide state —
/// the licence setting and its font cache. Rendering from several threads at
/// once produced documents whose text came back subtly differently ordered, so
/// a test that read a phrase out of the page failed perhaps one run in three
/// and passed every time it was run on its own. Nothing was wrong with the card
/// or the assertion; the renders were simply stepping on each other.
/// </para>
/// </summary>
[CollectionDefinition(Name)]
public class PdfRenderCollection
{
    public const string Name = "report card rendering";
}
