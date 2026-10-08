using Microsoft.EntityFrameworkCore;
using psms.Academic.Students;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Academic;

/// <summary>
/// Putting a learner in a class records which year that was.
/// <para>
/// <c>Student.CurrentClassId</c> is a single value with no year attached. It is
/// overwritten each time a learner moves, so it answers "where is this learner
/// now" and nothing else — and nothing was writing the year-by-year record
/// beside it.
/// </para>
/// <para>
/// That made every question of the form "what did this learner do in year X"
/// unanswerable: how many years they have been in a phase, whether they have
/// already repeated one, what the district schedule has to show. The retention
/// warning read that history, found nothing, and stayed silent for every
/// learner in the school.
/// </para>
/// </summary>
public class EnrolmentHistory_Tests : psmsTestBase
{
    private readonly IStudentAppService _students;

    private Guid _gradeId;
    private Guid _classA;
    private Guid _classB;
    private Guid _nextGradeClass;
    private Guid _yearId;
    private Guid _studentId;

    public EnrolmentHistory_Tests()
    {
        _students = Resolve<IStudentAppService>();
        Seed();
    }

    private void Seed()
    {
        _yearId = Guid.NewGuid();
        _gradeId = Guid.NewGuid();
        _classA = Guid.NewGuid();
        _classB = Guid.NewGuid();
        _nextGradeClass = Guid.NewGuid();
        _studentId = Guid.NewGuid();
        var nextGradeId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            var year = new AcademicYear(_yearId, 1, 2026,
                new DateTime(2026, 1, 1), new DateTime(2026, 12, 1));
            year.IsCurrent = true;
            context.AcademicYears.Add(year);

            context.Grades.Add(new Grade(_gradeId, 1,
                SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Grades.Add(new Grade(nextGradeId, 1,
                SouthAfricanGradeLevel.Grade9, "Grade 9", SouthAfricanSchoolPhase.Senior));

            context.Classes.Add(new Class(_classA, 1, "8A", _gradeId, _yearId, 30));
            context.Classes.Add(new Class(_classB, 1, "8B", _gradeId, _yearId, 30));
            context.Classes.Add(new Class(_nextGradeClass, 1, "9A", nextGradeId, _yearId, 30));

            context.Students.Add(new Student(_studentId, 1, "Ayanda", "Nkosi",
                new DateTime(2012, 1, 1), Gender.Female, "A001", DateTime.Today, _gradeId, _classA));

            context.SaveChanges();
        });
    }

    private int RowsFor(Guid studentId) => UsingDbContext(1, context =>
        context.StudentClasses.Count(sc => sc.StudentId == studentId));

    private StudentClass CurrentRow(Guid studentId) => UsingDbContext(1, context =>
        context.StudentClasses.FirstOrDefault(sc => sc.StudentId == studentId && sc.IsCurrent));

    [Fact]
    public async Task Placing_a_learner_in_a_class_records_the_year()
    {
        RowsFor(_studentId).ShouldBe(0, "nothing has written the history yet");

        await _students.AssignClassAsync(_studentId, _classA);

        RowsFor(_studentId).ShouldBe(1);

        var row = CurrentRow(_studentId);
        row.ShouldNotBeNull();
        row.ClassId.ShouldBe(_classA);
        row.AcademicYearId.ShouldBe(_yearId);
    }

    [Fact]
    public async Task Moving_a_learner_within_the_year_is_a_correction_not_a_second_enrolment()
    {
        // 8A to 8B in the same year is the school fixing where a learner sits,
        // not the learner having been in two classes that year.
        await _students.AssignClassAsync(_studentId, _classA);
        await _students.AssignClassAsync(_studentId, _classB);

        RowsFor(_studentId).ShouldBe(1);
        CurrentRow(_studentId).ClassId.ShouldBe(_classB);
    }

    [Fact]
    public async Task Placing_them_in_the_same_class_twice_changes_nothing()
    {
        await _students.AssignClassAsync(_studentId, _classA);
        await _students.AssignClassAsync(_studentId, _classA);

        RowsFor(_studentId).ShouldBe(1);
    }

    [Fact]
    public async Task The_backfill_records_this_year_for_a_learner_who_has_a_class_but_no_history()
    {
        // The learner was seeded straight into a class, which is the state
        // every existing learner is in.
        RowsFor(_studentId).ShouldBe(0);

        var written = await _students.BackfillCurrentYearEnrolmentsAsync();

        written.ShouldBe(1);
        CurrentRow(_studentId).ClassId.ShouldBe(_classA);
    }

    [Fact]
    public async Task The_backfill_can_be_run_twice_without_duplicating()
    {
        await _students.BackfillCurrentYearEnrolmentsAsync();
        var second = await _students.BackfillCurrentYearEnrolmentsAsync();

        second.ShouldBe(0);
        RowsFor(_studentId).ShouldBe(1);
    }

    [Fact]
    public async Task The_backfill_leaves_a_learner_the_assignment_already_recorded()
    {
        await _students.AssignClassAsync(_studentId, _classB);

        var written = await _students.BackfillCurrentYearEnrolmentsAsync();

        written.ShouldBe(0);
        CurrentRow(_studentId).ClassId.ShouldBe(_classB);
    }
}
