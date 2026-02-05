# PSMS Entity Implementation Plan
## ASP.NET Boilerplate (aspnetboilerplate.com) v10.2.0

**CRITICAL**: Using **ASP.NET Boilerplate** (Abp.*), NOT ABP vNext (Volo.Abp.*)

---

## ABP Boilerplate Base Classes

### Entity Base Classes (from Abp.Domain.Entities)
```csharp
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

// Basic entity
public class MyEntity : Entity<Guid> { }

// Creation audited (CreationTime, CreatorUserId)
public class MyEntity : CreationAuditedEntity<Guid> { }

// Full audited (Creation + Modification + Deletion)
public class MyEntity : FullAuditedEntity<Guid> { }

// With soft delete
public class MyEntity : FullAuditedEntity<Guid>, ISoftDelete { }
```

### Multi-Tenancy Interfaces
```csharp
using Abp.Domain.Entities;

// Required tenant (int TenantId - NOT nullable)
public class MyEntity : Entity<Guid>, IMustHaveTenant
{
    public int TenantId { get; set; }
}

// Optional tenant (int? TenantId - nullable)
public class MyEntity : Entity<Guid>, IMayHaveTenant
{
    public int? TenantId { get; set; }
}
```

### Auditing Properties (Automatic)
```csharp
// CreationAuditedEntity provides:
public DateTime CreationTime { get; set; }
public long? CreatorUserId { get; set; }

// FullAuditedEntity adds:
public DateTime? LastModificationTime { get; set; }
public long? LastModifierUserId { get; set; }
public DateTime? DeletionTime { get; set; }
public long? DeleterUserId { get; set; }
public bool IsDeleted { get; set; }
```

---

## Entity Implementation Pattern

### Aggregate Root Pattern
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a student enrolled in the school
    /// </summary>
    public class Student : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        // Constants
        public const int MaxFirstNameLength = 50;
        public const int MaxLastNameLength = 50;

        // Multi-tenancy (IMayHaveTenant)
        public int? TenantId { get; set; }

        // Properties
        [Required]
        [StringLength(MaxFirstNameLength)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(MaxLastNameLength)]
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        // Foreign Keys
        public Guid CurrentGradeId { get; set; }
        public Guid CurrentClassId { get; set; }

        // Navigation Properties
        [ForeignKey("CurrentGradeId")]
        public virtual Grade Grade { get; set; }

        [ForeignKey("CurrentClassId")]
        public virtual Class Class { get; set; }

        // Collections
        public virtual ICollection<StudentParent> StudentParents { get; set; }
        public virtual ICollection<Mark> Marks { get; set; }

        // Constructors
        protected Student()
        {
            // Required for EF Core
        }

        public Student(Guid id, int? tenantId, string firstName, string lastName)
            : base(id)
        {
            TenantId = tenantId;
            FirstName = firstName;
            LastName = lastName;

            StudentParents = new List<StudentParent>();
            Marks = new List<Mark>();
        }

        // Business Logic Methods (optional)
        public void UpdateName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
```

---

## Module Structure

### 1. Domain/Shared (Foundation)

#### Value Objects
- **Address.cs** - South African address (Street, Suburb, City, Province, PostalCode)
- **Money.cs** - Amount + Currency (ZAR)
- **SouthAfricanIdNumber.cs** - With Luhn validation

#### Enumerations
- **Enumerations.cs** - All enums in one file (30+ enums)

---

### 2. Domain/Academic (16 entities)

#### Aggregate Roots (FullAuditedEntity<Guid>, IMayHaveTenant)
1. **Student** - Learner information
2. **Teacher** - Teacher information
3. **Parent** - Parent/Guardian information
4. **Grade** - Grade levels (R-12)
5. **Class** - Class within grade
6. **Subject** - Academic subjects
7. **AcademicYear** - School year (Jan-Dec)
8. **Term** - 4 terms per year
9. **Timetable** - Class schedule
10. **POPIAConsent** - Data protection consent

#### Child Entities (CreationAuditedEntity<Guid>)
11. **TermEvent** - Events within term
12. **TimetableSlot** - Period in timetable

#### Junction Tables (Entity<Guid>)
13. **StudentParent** - Student-Parent relationship
14. **StudentSubject** - Student-Subject enrollment
15. **TeacherSubject** - Teacher-Subject assignment
16. **TeacherClass** - Teacher-Class assignment
17. **GradeSubject** - Grade-Subject offering

---

### 3. Domain/Admissions (7 entities)

#### Aggregate Roots
1. **Application** - School application (FullAuditedEntity<Guid>, IMayHaveTenant)
2. **Waitlist** - Waitlist management (FullAuditedEntity<Guid>, IMayHaveTenant)

#### Child Entities (Entity<Guid> or CreationAuditedEntity<Guid>)
3. **ApplicantParent** - Prospective parent info
4. **ApplicationDocument** - Uploaded documents
5. **ApplicationFee** - Application payment
6. **AdmissionInterview** - Interview details
7. **AdmissionAssessment** - Placement test

---

### 4. Domain/Financial (4 entities)

#### Aggregate Roots
1. **FeeStructure** - Fee definition per grade (FullAuditedEntity<Guid>, IMayHaveTenant)
2. **Payment** - Payment transactions (FullAuditedEntity<Guid>, IMayHaveTenant)
3. **StudentFee** - Fees assigned to student (FullAuditedEntity<Guid>, IMayHaveTenant)

#### Junction/Child
4. **PaymentAllocation** - Payment-to-Fee allocation (Entity<Guid>)

---

### 5. Domain/Assessment (6 entities)

#### Aggregate Roots
1. **Assessment** - Quiz/Test/Exam (FullAuditedEntity<Guid>, IMayHaveTenant)
2. **Mark** - Student mark (FullAuditedEntity<Guid>, IMayHaveTenant)
3. **Report** - Term report (FullAuditedEntity<Guid>, IMayHaveTenant)

#### Child Entities
4. **AssessmentQuestion** - Questions in assessment (Entity<Guid>)
5. **StudentAnswer** - Student's answer (Entity<Guid>)
6. **ReportSubject** - Subject section in report (Entity<Guid>)

---

### 6. Domain/Learning (2 entities)

#### Aggregate Roots
1. **LearningMaterial** - Uploaded content (FullAuditedEntity<Guid>, IMayHaveTenant)
2. **OnlineLesson** - Virtual class (FullAuditedEntity<Guid>, IMayHaveTenant)

---

### 7. Domain/Communication (5 entities)

#### Aggregate Roots
1. **Announcement** - School announcements (FullAuditedEntity<Guid>, IMayHaveTenant)
2. **Message** - Direct messages (FullAuditedEntity<Guid>, IMayHaveTenant)
3. **Notification** - System notifications (CreationAuditedEntity<Guid>)
4. **Document** - School documents (FullAuditedEntity<Guid>, IMayHaveTenant)

#### Child Entities
5. **AnnouncementRead** - Read tracking (Entity<Guid>)

---

### 8. Domain/SASpecific (6 entities)

#### Aggregate Roots
1. **SchoolTransport** - Bus routes (FullAuditedEntity<Guid>, IMayHaveTenant)
2. **AfterCare** - After-school care (FullAuditedEntity<Guid>, IMayHaveTenant)
3. **ExtramuralActivity** - Extra activities (FullAuditedEntity<Guid>, IMayHaveTenant)

#### Junction Tables
4. **StudentTransport** - Student-Transport link (Entity<Guid>)
5. **StudentAfterCare** - Student-AfterCare link (Entity<Guid>)
6. **StudentExtramural** - Student-Extramural link (Entity<Guid>)

---

### 9. Domain/Attendance (1 entity)

#### Aggregate Roots
1. **Attendance** - Daily attendance (CreationAuditedEntity<Guid>, IMayHaveTenant)

---

## DbContext Updates Required

Add DbSet properties to **psmsDbContext.cs**:

```csharp
// Academic
public DbSet<Student> Students { get; set; }
public DbSet<Teacher> Teachers { get; set; }
public DbSet<Parent> Parents { get; set; }
public DbSet<Grade> Grades { get; set; }
public DbSet<Class> Classes { get; set; }
public DbSet<Subject> Subjects { get; set; }
public DbSet<AcademicYear> AcademicYears { get; set; }
public DbSet<Term> Terms { get; set; }
public DbSet<Timetable> Timetables { get; set; }
public DbSet<POPIAConsent> POPIAConsents { get; set; }

// Admissions
public DbSet<Application> Applications { get; set; }
public DbSet<ApplicantParent> ApplicantParents { get; set; }
public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }
public DbSet<ApplicationFee> ApplicationFees { get; set; }
public DbSet<AdmissionInterview> AdmissionInterviews { get; set; }
public DbSet<AdmissionAssessment> AdmissionAssessments { get; set; }
public DbSet<Waitlist> Waitlists { get; set; }

// Financial
public DbSet<FeeStructure> FeeStructures { get; set; }
public DbSet<StudentFee> StudentFees { get; set; }
public DbSet<Payment> Payments { get; set; }

// Assessment
public DbSet<Assessment> Assessments { get; set; }
public DbSet<Mark> Marks { get; set; }
public DbSet<Report> Reports { get; set; }

// Learning
public DbSet<LearningMaterial> LearningMaterials { get; set; }
public DbSet<OnlineLesson> OnlineLessons { get; set; }

// Communication
public DbSet<Announcement> Announcements { get; set; }
public DbSet<Message> Messages { get; set; }
public DbSet<Notification> Notifications { get; set; }
public DbSet<Document> Documents { get; set; }

// SASpecific
public DbSet<SchoolTransport> SchoolTransports { get; set; }
public DbSet<AfterCare> AfterCares { get; set; }
public DbSet<ExtramuralActivity> ExtramuralActivities { get; set; }

// Attendance
public DbSet<Attendance> Attendances { get; set; }
```

---

## Entity Fluent Configuration (Optional but Recommended)

Create configuration classes in **EntityFrameworkCore/Configurations/**:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using psms.Domain.Academic.Entities;

namespace psms.EntityFrameworkCore.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");

            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.AdmissionNumber).IsUnique();
            builder.HasIndex(e => e.CurrentGradeId);
            builder.HasIndex(e => e.CurrentClassId);

            builder.HasOne(e => e.Grade)
                .WithMany()
                .HasForeignKey(e => e.CurrentGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(e => e.CurrentClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(e => e.PhysicalAddress);
        }
    }
}
```

---

## Implementation Steps

### Step 1: Create Shared Foundation
1. Create Value Objects (Address, Money, SouthAfricanIdNumber)
2. Create all Enumerations

### Step 2: Create Academic Module (Priority 1)
Core entities needed for basic school operations

### Step 3: Create Admissions Module (Priority 2)
Student application and enrollment process

### Step 4: Create Financial Module (Priority 3)
Fee management and payments

### Step 5: Create Assessment Module (Priority 4)
Grading and reporting

### Step 6: Create Learning Module (Priority 5)
Content delivery

### Step 7: Create Communication Module (Priority 6)
Announcements and messaging

### Step 8: Create SASpecific Module (Priority 7)
South African extras (transport, aftercare, extramurals)

### Step 9: Update DbContext
Add all DbSet properties

### Step 10: Create Entity Configurations
Fluent API configurations for complex relationships

### Step 11: Generate Migration
```bash
Add-Migration InitialCreate
Update-Database
```

---

## Key Differences: ABP Boilerplate vs ABP vNext

| Feature | ABP Boilerplate (aspnetboilerplate.com) | ABP vNext (abp.io) |
|---------|----------------------------------------|-------------------|
| **Namespace** | `Abp.*` | `Volo.Abp.*` |
| **Base Entity** | `Abp.Domain.Entities.Entity<T>` | `Volo.Abp.Domain.Entities.Entity<T>` |
| **Full Audited** | `FullAuditedEntity<T>` | `FullAuditedAggregateRoot<T>` |
| **Multi-Tenancy** | `IMayHaveTenant` (int? TenantId) | `IMultiTenant` (Guid? TenantId) |
| **Required Tenant** | `IMustHaveTenant` (int TenantId) | N/A |
| **User ID Type** | `long` | `Guid` |
| **Tenant ID Type** | `int` or `int?` | `Guid?` |
| **Soft Delete** | `ISoftDelete` interface | Same |
| **DbContext Base** | `AbpZeroDbContext<Tenant, Role, User>` | `AbpDbContext<T>` |

---

## CRITICAL: Data Types in ABP Boilerplate

```csharp
// ✅ CORRECT for ABP Boilerplate
public int? TenantId { get; set; }        // Nullable int for optional tenant
public int TenantId { get; set; }         // Required int for must-have tenant
public long? CreatorUserId { get; set; }  // User IDs are long
public Guid Id { get; set; }              // Entity IDs are Guid

// ❌ WRONG (These are for ABP vNext/Volo.Abp)
public Guid? TenantId { get; set; }       // NO! Guid TenantId is ABP vNext
```

---

**Total Entities to Create**: 49
**Estimated Time**: 6-8 hours for all entities
**Complexity**: Medium-High

---

**Next Step**: Shall I proceed to create all entities using proper ABP Boilerplate patterns?
