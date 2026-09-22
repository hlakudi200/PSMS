using System;
using System.Linq;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using psms.Academic.Attendances;
using psms.Academic.Attendances.Dto;
using psms.Academic.Parents;
using psms.Academic.Shared;
using psms.Academic.Students;
using psms.Academic.StudentParents;
using psms.Assessment.Reports;
using psms.Assessment.Reports.Dto;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Academic;

/// <summary>
/// #214 / MOB-BE-04: a parent may only read their own children's data.
/// Seeds two families (Parent A ↔ Student A, Parent B ↔ Student B) in the
/// same tenant and class, then verifies the ICurrentParentResolver guard
/// added to the 12 student-scoped services via representative call sites
/// (single-record read, list filtering, and a write path), plus that
/// staff and student-self behavior is unchanged.
/// </summary>
public class ParentDataScoping_Tests : psmsTestBase
{
    private readonly ICurrentParentResolver _currentParentResolver;
    private readonly IStudentAppService _studentAppService;
    private readonly IAttendanceAppService _attendanceAppService;
    private readonly IReportAppService _reportAppService;
    private readonly IStudentParentAppService _studentParentAppService;

    private Guid _studentAId;
    private Guid _studentBId;
    private Guid _classId;
    private Guid _teacherId;
    private long _studentAUserId;
    private long _parentAUserId;
    private long _parentBUserId;
    private long _parentNoChildrenUserId;

    public ParentDataScoping_Tests()
    {
        _currentParentResolver = Resolve<ICurrentParentResolver>();
        _studentAppService = Resolve<IStudentAppService>();
        _attendanceAppService = Resolve<IAttendanceAppService>();
        _reportAppService = Resolve<IReportAppService>();
        _studentParentAppService = Resolve<IStudentParentAppService>();

        SeedFamilies();
    }

    private void SeedFamilies()
    {
        var studentAUser = CreateUser("student.a");
        var studentBUser = CreateUser("student.b");
        var parentAUser = CreateUser("parent.a");
        var parentBUser = CreateUser("parent.b");
        var parentNoChildrenUser = CreateUser("parent.nokids");
        var teacherUser = CreateUser("teacher.a");

        _studentAUserId = studentAUser.Id;
        _parentAUserId = parentAUser.Id;
        _parentBUserId = parentBUser.Id;
        _parentNoChildrenUserId = parentNoChildrenUser.Id;

        AssignRole(studentAUser.Id, StaticRoleNames.Tenants.Student);
        AssignRole(studentBUser.Id, StaticRoleNames.Tenants.Student);
        AssignRole(parentAUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(parentBUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(parentNoChildrenUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(teacherUser.Id, StaticRoleNames.Tenants.Teacher);

        var academicYearId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        _classId = Guid.NewGuid();
        _teacherId = Guid.NewGuid();
        _studentAId = Guid.NewGuid();
        _studentBId = Guid.NewGuid();
        var parentAId = Guid.NewGuid();
        var parentBId = Guid.NewGuid();

        UsingDbContext(context =>
        {
            context.AcademicYears.Add(new AcademicYear(academicYearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 1)));
            context.Grades.Add(new Grade(gradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Classes.Add(new Class(_classId, 1, "8A", gradeId, academicYearId, 30));
            context.Teachers.Add(new Teacher(_teacherId, 1, teacherUser.Id, "Teach", "Er", "EMP001", "teacher.a@test.local"));

            context.Students.Add(new Student(_studentAId, 1, "Ann", "A-Family", new DateTime(2012, 1, 1), Gender.Female, "A001", DateTime.Today, gradeId, _classId) { UserId = studentAUser.Id });
            context.Students.Add(new Student(_studentBId, 1, "Ben", "B-Family", new DateTime(2012, 1, 1), Gender.Male, "B001", DateTime.Today, gradeId, _classId) { UserId = studentBUser.Id });

            context.Parents.Add(new Parent(parentAId, 1, parentAUser.Id, "Parent", "A-Family", "parent.a@test.local", "0710000000"));
            context.Parents.Add(new Parent(parentBId, 1, parentBUser.Id, "Parent", "B-Family", "parent.b@test.local", "0720000000"));
            // Parent with zero StudentParent links.
            context.Parents.Add(new Parent(Guid.NewGuid(), 1, parentNoChildrenUser.Id, "Parent", "NoKids", "parent.nokids@test.local", "0730000000"));

            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _studentAId, parentAId, RelationshipType.Guardian));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _studentBId, parentBId, RelationshipType.Guardian));

            context.Attendances.Add(new Attendance(Guid.NewGuid(), 1, _studentAId, _classId, _teacherId, DateTime.Today, AttendanceStatus.Present));
            context.Attendances.Add(new Attendance(Guid.NewGuid(), 1, _studentBId, _classId, _teacherId, DateTime.Today, AttendanceStatus.Present));

            var reportB = new Report(Guid.NewGuid(), 1, _studentBId, _classId, academicYearId, ReportType.Term1) { Status = ReportStatus.Published };
            context.Reports.Add(reportB);
        });
    }

    private User CreateUser(string userName)
    {
        return UsingDbContext(context =>
        {
            var user = new User
            {
                TenantId = 1,
                UserName = userName,
                Name = userName,
                Surname = "Test",
                EmailAddress = $"{userName}@test.local",
                IsActive = true,
                IsEmailConfirmed = true,
            };
            user.SetNormalizedNames();
            user.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()))
                .HashPassword(user, "Test123!");
            context.Users.Add(user);
            return user;
        });
    }

    private void AssignRole(long userId, string roleName)
    {
        UsingDbContext(context =>
        {
            var role = context.Roles.IgnoreQueryFilters().First(r => r.TenantId == 1 && r.Name == roleName);
            context.UserRoles.Add(new UserRole(1, userId, role.Id));
        });
    }

    private void LoginAs(long userId)
    {
        AbpSession.TenantId = 1;
        AbpSession.UserId = userId;
    }

    // ---- CurrentParentResolver ----

    [Fact]
    public async Task Resolver_Returns_Linked_Children_For_Parent()
    {
        LoginAs(_parentAUserId);

        var childIds = await _currentParentResolver.GetCurrentChildStudentIdsAsync();

        childIds.ShouldNotBeNull();
        childIds.ShouldBe(new[] { _studentAId });
    }

    [Fact]
    public async Task Resolver_Returns_Null_For_Student_Caller()
    {
        LoginAs(_studentAUserId);

        var childIds = await _currentParentResolver.GetCurrentChildStudentIdsAsync();

        childIds.ShouldBeNull();
    }

    [Fact]
    public async Task Resolver_Returns_Null_For_Staff_Caller()
    {
        LoginAsDefaultTenantAdmin();

        var childIds = await _currentParentResolver.GetCurrentChildStudentIdsAsync();

        childIds.ShouldBeNull();
    }

    [Fact]
    public async Task Resolver_Returns_Empty_List_For_Parent_With_No_Links()
    {
        LoginAs(_parentNoChildrenUserId);

        var childIds = await _currentParentResolver.GetCurrentChildStudentIdsAsync();

        childIds.ShouldNotBeNull();
        childIds.ShouldBeEmpty();
    }

    // ---- StudentAppService: single-record + list filtering ----

    [Fact]
    public async Task Parent_Can_Read_Own_Child_Student_Record()
    {
        LoginAs(_parentAUserId);

        var student = await _studentAppService.GetAsync(_studentAId);

        student.Id.ShouldBe(_studentAId);
    }

    [Fact]
    public async Task Parent_Cannot_Read_Another_Familys_Student_Record()
    {
        LoginAs(_parentAUserId);

        await Should.ThrowAsync<UserFriendlyException>(() => _studentAppService.GetAsync(_studentBId));
    }

    [Fact]
    public async Task Parent_List_Is_Filtered_To_Own_Children_Only()
    {
        LoginAs(_parentAUserId);

        var result = await _studentAppService.GetAllAsync(new GetAcademicEntityInput());

        result.Items.Select(s => s.Id).ShouldBe(new[] { _studentAId });
    }

    [Fact]
    public async Task Staff_List_Is_Unaffected_And_Sees_All_Students()
    {
        LoginAsDefaultTenantAdmin();

        var result = await _studentAppService.GetAllAsync(new GetAcademicEntityInput());

        result.Items.Select(s => s.Id).ShouldContain(_studentAId);
        result.Items.Select(s => s.Id).ShouldContain(_studentBId);
    }

    [Fact]
    public async Task Student_Self_Access_Is_Unaffected()
    {
        LoginAs(_studentAUserId);

        var own = await _studentAppService.GetAsync(_studentAId);
        own.Id.ShouldBe(_studentAId);

        await Should.ThrowAsync<UserFriendlyException>(() => _studentAppService.GetAsync(_studentBId));
    }

    // ---- AttendanceAppService: allowed / denied / list filtering ----

    [Fact]
    public async Task Parent_Can_Read_Own_Childs_Attendance()
    {
        LoginAs(_parentAUserId);

        var records = await _attendanceAppService.GetByStudentAsync(_studentAId, null, null);

        records.Items.ShouldNotBeEmpty();
        records.Items.All(a => a.StudentId == _studentAId).ShouldBeTrue();
    }

    [Fact]
    public async Task Parent_Cannot_Read_Another_Familys_Attendance()
    {
        LoginAs(_parentAUserId);

        var records = await _attendanceAppService.GetByStudentAsync(_studentBId, null, null);

        records.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Parent_Attendance_List_Is_Filtered_To_Own_Children_Only()
    {
        LoginAs(_parentAUserId);

        var result = await _attendanceAppService.GetAllAsync(new GetAttendanceInput());

        result.Items.ShouldNotBeEmpty();
        result.Items.All(a => a.StudentId == _studentAId).ShouldBeTrue();
    }

    // ---- ReportAppService.AcknowledgeByParentAsync: write path ----

    [Fact]
    public async Task Parent_Cannot_Acknowledge_Another_Familys_Report()
    {
        var reportId = UsingDbContext(context => context.Reports.First(r => r.StudentId == _studentBId).Id);

        LoginAs(_parentAUserId); // Parent A acting on Student B's report

        await Should.ThrowAsync<UserFriendlyException>(
            () => _reportAppService.AcknowledgeByParentAsync(reportId, new ReportCommentDto { Comment = "Looks good" }));
    }

    // ---- StudentParentAppService.GetMyChildrenAsync ----

    [Fact]
    public async Task GetMyChildren_Returns_Only_Callers_Own_Links()
    {
        LoginAs(_parentAUserId);

        var result = await _studentParentAppService.GetMyChildrenAsync();

        result.Items.Select(l => l.StudentId).ShouldBe(new[] { _studentAId });
    }

    [Fact]
    public async Task GetMyChildren_Returns_Empty_For_NonParent_Caller()
    {
        LoginAs(_studentAUserId);

        var result = await _studentParentAppService.GetMyChildrenAsync();

        result.Items.ShouldBeEmpty();
    }
}
