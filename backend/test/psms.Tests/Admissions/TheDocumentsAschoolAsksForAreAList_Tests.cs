using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// What a school asks for has to be sayable in the same words as what a family
/// uploads.
/// <para>
/// The school typed its requirements as free text — one live row read
/// "Parents ID \n Learners birth certificate \n Previous school report " —
/// while every upload carries a <see cref="DocumentCategory"/>. Nothing could
/// compare the two, so the required-documents status and the
/// <c>application.documents-verified</c> guard could not answer the one
/// question they exist for.
/// </para>
/// <para>
/// The column's own comment always said "JSON array of required document
/// categories". The screen offered a textarea, so prose is what it got.
/// </para>
/// </summary>
public class TheDocumentsAschoolAsksForAreAList_Tests
{
    [Fact]
    public void Nothing_set_is_not_a_requirement_for_nothing()
    {
        AdmissionSettings.ParseRequiredDocuments(null).ShouldBeEmpty();
        AdmissionSettings.ParseRequiredDocuments("   ").ShouldBeEmpty();
    }

    [Fact]
    public void A_json_array_reads_as_the_categories_it_names()
    {
        // The shape the column was documented as holding all along.
        var categories = AdmissionSettings.ParseRequiredDocuments(
            "[\"BirthCertificate\", \"PreviousSchoolReport\"]");

        categories.ShouldBe(new[]
        {
            DocumentCategory.BirthCertificate,
            DocumentCategory.PreviousSchoolReport,
        });
    }

    [Fact]
    public void A_plain_comma_separated_list_reads_too()
    {
        AdmissionSettings.ParseRequiredDocuments("ParentIdDocument,ProofOfResidence")
            .ShouldBe(new[] { DocumentCategory.ParentIdDocument, DocumentCategory.ProofOfResidence });
    }

    [Fact]
    public void Leftover_prose_means_the_school_has_not_chosen()
    {
        /* The real row from the live tenant. Reading nothing out of it is
           right: it must fall back to the default list for the grade, not be
           taken to mean this school requires nothing at all. */
        var categories = AdmissionSettings.ParseRequiredDocuments(
            "Parents ID \nLearners birth certificate \nPrevious school report ");

        categories.ShouldBeEmpty();
    }

    [Fact]
    public void A_word_it_recognises_inside_prose_is_still_taken()
    {
        // Half-migrated rows are the likely case, and taking what can be read
        // beats discarding the lot.
        AdmissionSettings.ParseRequiredDocuments("Parents ID\nProofOfResidence\nsomething else")
            .ShouldBe(new[] { DocumentCategory.ProofOfResidence });
    }

    [Fact]
    public void The_same_category_twice_is_asked_for_once()
    {
        AdmissionSettings.ParseRequiredDocuments("IdDocument, iddocument, ID_DOCUMENT")
            .ShouldBe(new[] { DocumentCategory.IdDocument });
    }

    [Fact]
    public void Writing_the_list_back_is_read_the_same_way()
    {
        var settings = new AdmissionSettings(Guid.NewGuid(), 1, Guid.NewGuid(), 0m)
        {
            RequiredDocumentCategories = new[]
            {
                DocumentCategory.BirthCertificate,
                DocumentCategory.ParentIdDocument,
                DocumentCategory.StudyPermit,
            },
        };

        settings.RequiredDocuments.ShouldBe("BirthCertificate,ParentIdDocument,StudyPermit");
        settings.RequiredDocumentCategories.Count.ShouldBe(3);
    }

    [Fact]
    public void An_empty_choice_clears_the_column_rather_than_writing_an_empty_string()
    {
        var settings = new AdmissionSettings(Guid.NewGuid(), 1, Guid.NewGuid(), 0m)
        {
            RequiredDocumentCategories = Array.Empty<DocumentCategory>(),
        };

        settings.RequiredDocuments.ShouldBeNull();
    }

    [Fact]
    public void The_stored_list_fits_the_column()
    {
        // Every category at once is the worst case, and it has to fit.
        var everything = Enum.GetValues(typeof(DocumentCategory))
            .Cast<DocumentCategory>()
            .ToArray();

        var settings = new AdmissionSettings(Guid.NewGuid(), 1, Guid.NewGuid(), 0m)
        {
            RequiredDocumentCategories = everything,
        };

        settings.RequiredDocuments.Length.ShouldBeLessThan(AdmissionSettings.MaxRequiredDocumentsLength);
    }
}
