using Abp.Zero.EntityFrameworkCore;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

// Domain Entities
using psms.Domain.Academic.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Learning.Entities;
using psms.Domain.Communication.Entities;
using psms.Domain.SASpecific.Entities;
using psms.Domain.Shared.ValueObjects;

namespace psms.EntityFrameworkCore;

public class psmsDbContext : AbpZeroDbContext<Tenant, Role, User, psmsDbContext>
{
    /* ==================== Academic Module ==================== */

    /// <summary>
    /// Academic years (e.g., 2024, 2025)
    /// </summary>
    public DbSet<AcademicYear> AcademicYears { get; set; }

    /// <summary>
    /// Grade levels (Grade R - Grade 12)
    /// </summary>
    public DbSet<Grade> Grades { get; set; }

    /// <summary>
    /// Classes within grades (e.g., Grade 1A, Grade 1B)
    /// </summary>
    public DbSet<Class> Classes { get; set; }

    /// <summary>
    /// Subjects taught in the school
    /// </summary>
    public DbSet<Subject> Subjects { get; set; }

    /// <summary>
    /// Teachers/educators
    /// </summary>
    public DbSet<Teacher> Teachers { get; set; }

    /// <summary>
    /// Students enrolled in the school
    /// </summary>
    public DbSet<Student> Students { get; set; }

    /// <summary>
    /// Parents/guardians
    /// </summary>
    public DbSet<Parent> Parents { get; set; }

    /// <summary>
    /// Student-Parent relationships
    /// </summary>
    public DbSet<StudentParent> StudentParents { get; set; }

    /// <summary>
    /// Student class enrollments
    /// </summary>
    public DbSet<StudentClass> StudentClasses { get; set; }

    /// <summary>
    /// Class-Subject associations with teacher assignments
    /// </summary>
    public DbSet<ClassSubject> ClassSubjects { get; set; }

    /// <summary>
    /// Daily attendance records
    /// </summary>
    public DbSet<Attendance> Attendances { get; set; }

    /// <summary>
    /// Academic terms (4 per year in SA)
    /// </summary>
    public DbSet<Term> Terms { get; set; }

    /// <summary>
    /// Events within terms (holidays, exams, etc.)
    /// </summary>
    public DbSet<TermEvent> TermEvents { get; set; }

    /// <summary>
    /// Class timetables
    /// </summary>
    public DbSet<Timetable> Timetables { get; set; }

    /// <summary>
    /// Individual timetable slots/periods
    /// </summary>
    public DbSet<TimetableSlot> TimetableSlots { get; set; }

    /// <summary>
    /// POPIA consent records
    /// </summary>
    public DbSet<POPIAConsent> POPIAConsents { get; set; }

    /// <summary>
    /// Student medical information
    /// </summary>
    public DbSet<MedicalInfo> MedicalInfos { get; set; }

    /// <summary>
    /// Emergency contacts for students
    /// </summary>
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }

    /* ==================== Admissions Module ==================== */

    /// <summary>
    /// Admission applications
    /// </summary>
    public DbSet<Application> Applications { get; set; }

    /// <summary>
    /// Parent/guardian info on applications
    /// </summary>
    public DbSet<ApplicantParent> ApplicantParents { get; set; }

    /// <summary>
    /// Documents submitted with applications
    /// </summary>
    public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }

    /// <summary>
    /// Application fee payments
    /// </summary>
    public DbSet<ApplicationFee> ApplicationFees { get; set; }

    /// <summary>
    /// Admission interviews
    /// </summary>
    public DbSet<AdmissionInterview> AdmissionInterviews { get; set; }

    /// <summary>
    /// Placement assessments
    /// </summary>
    public DbSet<AdmissionAssessment> AdmissionAssessments { get; set; }

    /// <summary>
    /// Waitlist entries
    /// </summary>
    public DbSet<Waitlist> Waitlists { get; set; }

    /* ==================== Financial Module ==================== */

    /// <summary>
    /// Fee structures by grade/year
    /// </summary>
    public DbSet<FeeStructure> FeeStructures { get; set; }

    /// <summary>
    /// Individual student fees
    /// </summary>
    public DbSet<StudentFee> StudentFees { get; set; }

    /// <summary>
    /// Payment records
    /// </summary>
    public DbSet<Payment> Payments { get; set; }

    /// <summary>
    /// Payment allocations to fees
    /// </summary>
    public DbSet<PaymentAllocation> PaymentAllocations { get; set; }

    /* ==================== Assessment Module ==================== */

    /// <summary>
    /// Assessments (tests, exams, assignments)
    /// </summary>
    public DbSet<Assessment> Assessments { get; set; }

    /// <summary>
    /// Student marks for assessments
    /// </summary>
    public DbSet<Mark> Marks { get; set; }

    /// <summary>
    /// Term/year reports
    /// </summary>
    public DbSet<Report> Reports { get; set; }

    /// <summary>
    /// Assessment questions
    /// </summary>
    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }

    /// <summary>
    /// Student answers to questions
    /// </summary>
    public DbSet<StudentAnswer> StudentAnswers { get; set; }

    /// <summary>
    /// Subject-level report details
    /// </summary>
    public DbSet<ReportSubject> ReportSubjects { get; set; }

    /* ==================== Learning Module ==================== */

    /// <summary>
    /// Learning materials/resources
    /// </summary>
    public DbSet<LearningMaterial> LearningMaterials { get; set; }

    /// <summary>
    /// Online/virtual lessons
    /// </summary>
    public DbSet<OnlineLesson> OnlineLessons { get; set; }

    /* ==================== Communication Module ==================== */

    /// <summary>
    /// School announcements
    /// </summary>
    public DbSet<Announcement> Announcements { get; set; }

    /// <summary>
    /// Direct messages between users
    /// </summary>
    public DbSet<Message> Messages { get; set; }

    /// <summary>
    /// User notifications (custom domain notifications, hides ABP base)
    /// </summary>
    public new DbSet<Notification> Notifications { get; set; }

    /// <summary>
    /// Shared documents
    /// </summary>
    public DbSet<Document> Documents { get; set; }

    /// <summary>
    /// Announcement read tracking
    /// </summary>
    public DbSet<AnnouncementRead> AnnouncementReads { get; set; }

    /* ==================== SA Specific Module ==================== */

    /// <summary>
    /// School transport routes
    /// </summary>
    public DbSet<SchoolTransport> SchoolTransports { get; set; }

    /// <summary>
    /// After-care programs
    /// </summary>
    public DbSet<AfterCare> AfterCares { get; set; }

    /// <summary>
    /// Extramural activities
    /// </summary>
    public DbSet<ExtramuralActivity> ExtramuralActivities { get; set; }

    /// <summary>
    /// Student transport enrollments
    /// </summary>
    public DbSet<StudentTransport> StudentTransports { get; set; }

    /// <summary>
    /// Student after-care enrollments
    /// </summary>
    public DbSet<StudentAfterCare> StudentAfterCares { get; set; }

    /// <summary>
    /// Student extramural enrollments
    /// </summary>
    public DbSet<StudentExtramural> StudentExtramurals { get; set; }

    public psmsDbContext(DbContextOptions<psmsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureAcademicModule(modelBuilder);
        ConfigureAdmissionsModule(modelBuilder);
        ConfigureFinancialModule(modelBuilder);
        ConfigureAssessmentModule(modelBuilder);
        ConfigureLearningModule(modelBuilder);
        ConfigureCommunicationModule(modelBuilder);
        ConfigureSASpecificModule(modelBuilder);
    }

    private void ConfigureAcademicModule(ModelBuilder modelBuilder)
    {
        // Configure owned types for Address value objects
        modelBuilder.Entity<Student>().OwnsOne(s => s.PhysicalAddress);
        modelBuilder.Entity<Student>().OwnsOne(s => s.PostalAddress);
        modelBuilder.Entity<Parent>().OwnsOne(p => p.Address);
        modelBuilder.Entity<Teacher>().OwnsOne(t => t.Address);

        // Student - unique admission number per tenant
        modelBuilder.Entity<Student>()
            .HasIndex(s => new { s.TenantId, s.AdmissionNumber })
            .IsUnique()
            .HasDatabaseName("IX_Students_TenantId_AdmissionNumber");

        // Teacher - unique employee number per tenant
        modelBuilder.Entity<Teacher>()
            .HasIndex(t => new { t.TenantId, t.EmployeeNumber })
            .IsUnique()
            .HasDatabaseName("IX_Teachers_TenantId_EmployeeNumber");

        // Class - unique class name per grade
        modelBuilder.Entity<Class>()
            .HasIndex(c => new { c.GradeId, c.ClassName })
            .IsUnique()
            .HasDatabaseName("IX_Classes_GradeId_ClassName");

        // StudentClass - prevent duplicate enrollments per academic year
        modelBuilder.Entity<StudentClass>()
            .HasIndex(sc => new { sc.StudentId, sc.ClassId, sc.AcademicYearId })
            .IsUnique()
            .HasDatabaseName("IX_StudentClasses_StudentId_ClassId_AcademicYearId");

        // StudentParent - prevent duplicate relationships
        modelBuilder.Entity<StudentParent>()
            .HasIndex(sp => new { sp.StudentId, sp.ParentId })
            .IsUnique()
            .HasDatabaseName("IX_StudentParents_StudentId_ParentId");

        // ClassSubject - prevent duplicate subject assignments
        modelBuilder.Entity<ClassSubject>()
            .HasIndex(cs => new { cs.ClassId, cs.SubjectId })
            .IsUnique()
            .HasDatabaseName("IX_ClassSubjects_ClassId_SubjectId");

        // Attendance - one record per student per day
        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.StudentId, a.AttendanceDate })
            .IsUnique()
            .HasDatabaseName("IX_Attendances_StudentId_AttendanceDate");
    }

    private void ConfigureAdmissionsModule(ModelBuilder modelBuilder)
    {
        // Application - unique reference number
        modelBuilder.Entity<Application>()
            .HasIndex(a => a.ApplicationNumber)
            .IsUnique()
            .HasDatabaseName("IX_Applications_ApplicationNumber");

        // Waitlist - one entry per application
        modelBuilder.Entity<Waitlist>()
            .HasIndex(w => w.ApplicationId)
            .IsUnique()
            .HasDatabaseName("IX_Waitlists_ApplicationId");
    }

    private void ConfigureFinancialModule(ModelBuilder modelBuilder)
    {
        // Payment - receipt number should be unique
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.ReceiptNumber)
            .IsUnique()
            .HasFilter("\"ReceiptNumber\" IS NOT NULL")
            .HasDatabaseName("IX_Payments_ReceiptNumber");

        // StudentFee - one fee per student per fee structure
        modelBuilder.Entity<StudentFee>()
            .HasIndex(sf => new { sf.StudentId, sf.FeeStructureId })
            .IsUnique()
            .HasDatabaseName("IX_StudentFees_StudentId_FeeStructureId");
    }

    private void ConfigureAssessmentModule(ModelBuilder modelBuilder)
    {
        // Mark - one mark per student per assessment
        modelBuilder.Entity<Mark>()
            .HasIndex(m => new { m.AssessmentId, m.StudentId })
            .IsUnique()
            .HasDatabaseName("IX_Marks_AssessmentId_StudentId");

        // Report - one report per student per term
        modelBuilder.Entity<Report>()
            .HasIndex(r => new { r.StudentId, r.TermId, r.ReportType })
            .IsUnique()
            .HasFilter("\"TermId\" IS NOT NULL")
            .HasDatabaseName("IX_Reports_StudentId_TermId_ReportType");

        // ReportSubject - one entry per report per subject
        modelBuilder.Entity<ReportSubject>()
            .HasIndex(rs => new { rs.ReportId, rs.SubjectId })
            .IsUnique()
            .HasDatabaseName("IX_ReportSubjects_ReportId_SubjectId");

        // StudentAnswer - one answer per student per question
        modelBuilder.Entity<StudentAnswer>()
            .HasIndex(sa => new { sa.AssessmentQuestionId, sa.StudentId })
            .IsUnique()
            .HasDatabaseName("IX_StudentAnswers_QuestionId_StudentId");
    }

    private void ConfigureLearningModule(ModelBuilder modelBuilder)
    {
        // No special configurations needed
    }

    private void ConfigureCommunicationModule(ModelBuilder modelBuilder)
    {
        // AnnouncementRead - one read per user per announcement
        modelBuilder.Entity<AnnouncementRead>()
            .HasIndex(ar => new { ar.AnnouncementId, ar.UserId })
            .IsUnique()
            .HasDatabaseName("IX_AnnouncementReads_AnnouncementId_UserId");

        // Message - index for conversation threads
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.ThreadId)
            .HasDatabaseName("IX_Messages_ThreadId");
    }

    private void ConfigureSASpecificModule(ModelBuilder modelBuilder)
    {
        // StudentTransport - one enrollment per student per transport per year
        modelBuilder.Entity<StudentTransport>()
            .HasIndex(st => new { st.StudentId, st.SchoolTransportId, st.AcademicYearId })
            .IsUnique()
            .HasDatabaseName("IX_StudentTransports_StudentId_TransportId_YearId");

        // StudentAfterCare - one enrollment per student per program per year
        modelBuilder.Entity<StudentAfterCare>()
            .HasIndex(sac => new { sac.StudentId, sac.AfterCareId, sac.AcademicYearId })
            .IsUnique()
            .HasDatabaseName("IX_StudentAfterCares_StudentId_AfterCareId_YearId");

        // StudentExtramural - one enrollment per student per activity per year
        modelBuilder.Entity<StudentExtramural>()
            .HasIndex(se => new { se.StudentId, se.ExtramuralActivityId, se.AcademicYearId })
            .IsUnique()
            .HasDatabaseName("IX_StudentExtramurals_StudentId_ActivityId_YearId");
    }
}
