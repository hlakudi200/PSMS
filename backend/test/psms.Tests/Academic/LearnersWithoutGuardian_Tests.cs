using psms.Academic.Students;
using psms.Academic.Students.Dto;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Academic;

/// <summary>
/// #437: the school office can list the learners with no parent or guardian
/// linked, the ones nobody can be contacted about and no report card can be
/// shown to. Every learner row says how many guardians it has.
/// </summary>
public class LearnersWithoutGuardian_Tests : psmsTestBase
{
    private readonly IStudentAppService _students;
    private readonly Guid _linkedId = Guid.NewGuid();
    private readonly Guid _unlinkedId = Guid.NewGuid();

    public LearnersWithoutGuardian_Tests()
    {
        _students = Resolve<IStudentAppService>();

        var academicYearId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var classId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(academicYearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 1)));
            context.Grades.Add(new Grade(gradeId, 1, SouthAfricanGradeLevel.Grade5, "Grade 5", SouthAfricanSchoolPhase.Intermediate));
            context.Classes.Add(new Class(classId, 1, "5A", gradeId, academicYearId, 30));
            context.Students.Add(new Student(_linkedId, 1, "Linked", "Learner", new DateTime(2016, 1, 1),
                Gender.Female, "G0001", DateTime.Today, gradeId, classId));
            context.Students.Add(new Student(_unlinkedId, 1, "Unlinked", "Learner", new DateTime(2016, 1, 1),
                Gender.Male, "G0002", DateTime.Today, gradeId, classId));
            context.Parents.Add(new Parent(parentId, 1, 0, "Guardian", "Learner", "g.learner@test.local", "0710000000"));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _linkedId, parentId, RelationshipType.Guardian));
            context.SaveChanges();
        });

        LoginAsDefaultTenantAdmin();
    }

    [Fact]
    public async Task Without_guardian_lists_only_the_learners_with_none()
    {
        var page = await _students.GetAllAsync(new GetStudentsInput { WithoutGuardian = true, MaxResultCount = 50 });

        page.Items.Select(s => s.Id).ShouldContain(_unlinkedId);
        page.Items.Select(s => s.Id).ShouldNotContain(_linkedId);
        page.Items.ShouldAllBe(s => s.ParentCount == 0);
        page.TotalCount.ShouldBe(page.Items.Count);
    }

    [Fact]
    public async Task The_unfiltered_list_still_has_everyone_with_their_guardian_count()
    {
        var page = await _students.GetAllAsync(new GetStudentsInput { MaxResultCount = 50 });

        page.Items.Single(s => s.Id == _linkedId).ParentCount.ShouldBe(1);
        page.Items.Single(s => s.Id == _unlinkedId).ParentCount.ShouldBe(0);
    }
}
