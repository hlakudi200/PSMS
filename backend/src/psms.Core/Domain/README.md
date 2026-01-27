# PSMS Domain Entities - Creation Summary

## Overview
This directory contains all domain entities for the Private School Management System (PSMS), following ASP.NET Boilerplate (aspnetboilerplate.com) patterns.

## Status: 18/49 Entities Complete

### ✅ COMPLETED MODULES

#### 1. Shared Foundation (4/4) - 100% Complete
Location: `Domain/Shared/`
- ✅ `ValueObjects/Address.cs` - SA address with Province, PostalCode
- ✅ `ValueObjects/Money.cs` - Amount (decimal) + Currency (default "ZAR")
- ✅ `ValueObjects/SouthAfricanIdNumber.cs` - With Luhn validation
- ✅ `Enums/Enumerations.cs` - ALL enums in ONE file (50+ enums)

#### 2. Academic Module (17/17) - 100% Complete
Location: `Domain/Academic/Entities/`

**Aggregate Roots** (10):
1. ✅ Student.cs
2. ✅ Teacher.cs
3. ✅ Parent.cs
4. ✅ Grade.cs
5. ✅ Class.cs
6. ✅ Subject.cs
7. ✅ AcademicYear.cs
8. ✅ Term.cs
9. ✅ Timetable.cs
10. ✅ POPIAConsent.cs

**Child Entities** (2):
11. ✅ TermEvent.cs
12. ✅ TimetableSlot.cs

**Junction Tables** (5):
13. ✅ StudentParent.cs
14. ✅ StudentSubject.cs
15. ✅ TeacherSubject.cs
16. ✅ TeacherClass.cs
17. ✅ GradeSubject.cs

**Attendance Module** (1):
18. ✅ Attendance.cs

### ⚠️ PENDING MODULES (31 entities)

#### 3. Admissions Module (0/7) - Placeholders Created
Location: `Domain/Admissions/Entities/`
- Application.cs, Waitlist.cs, ApplicantParent.cs
- ApplicationDocument.cs, ApplicationFee.cs
- AdmissionInterview.cs, AdmissionAssessment.cs

#### 4. Financial Module (0/4) - Placeholders Created
Location: `Domain/Financial/Entities/`
- FeeStructure.cs, StudentFee.cs
- Payment.cs, PaymentAllocation.cs

#### 5. Assessment Module (0/6) - Placeholders Created
Location: `Domain/Assessment/Entities/`
- Assessment.cs, Mark.cs, Report.cs
- AssessmentQuestion.cs, StudentAnswer.cs, ReportSubject.cs

#### 6. Learning Module (0/2) - Placeholders Created
Location: `Domain/Learning/Entities/`
- LearningMaterial.cs, OnlineLesson.cs

#### 7. Communication Module (0/5) - Placeholders Created
Location: `Domain/Communication/Entities/`
- Announcement.cs, Message.cs, Notification.cs
- Document.cs, AnnouncementRead.cs

#### 8. SASpecific Module (0/6) - Placeholders Created
Location: `Domain/SASpecific/Entities/`
- SchoolTransport.cs, AfterCare.cs, ExtramuralActivity.cs
- StudentTransport.cs, StudentAfterCare.cs, StudentExtramural.cs

## ASP.NET Boilerplate Patterns

All completed entities strictly follow ASP.NET Boilerplate (NOT ABP vNext) patterns:

### Framework
- ✅ Use `Abp.Domain.Entities` (NOT `Volo.Abp.Domain.Entities`)
- ✅ Use `Abp.Domain.Entities.Auditing`

### Multi-Tenancy
- ✅ Use `int? TenantId` (NOT Guid)
- ✅ Use `IMayHaveTenant` interface
- ✅ Use `IMustHaveTenant` interface (where required)

### Identity Integration
- ✅ Use `long` for UserId references (NOT Guid)
- ✅ ABP Identity User.Id is `long` type

### Base Classes
- ✅ `Entity<Guid>` - Simple entities
- ✅ `CreationAuditedEntity<Guid>` - Entities with creation audit
- ✅ `FullAuditedEntity<Guid>` - Entities with full audit trail

### Soft Delete
- ✅ `ISoftDelete` interface
- ✅ `IsDeleted` property (bool)

### Entity Structure
- ✅ Constants for max lengths
- ✅ Protected parameterless constructor for EF Core
- ✅ Public constructor with required parameters
- ✅ XML documentation comments on all members
- ✅ [Required] and [StringLength] data annotations
- ✅ [ForeignKey] attributes on navigation properties
- ✅ [Table] attribute with table name
- ✅ Collections initialized in constructor

## File Structure

```
Domain/
├── Shared/
│   ├── ValueObjects/
│   │   ├── Address.cs ✅
│   │   ├── Money.cs ✅
│   │   └── SouthAfricanIdNumber.cs ✅
│   └── Enums/
│       └── Enumerations.cs ✅
├── Academic/
│   └── Entities/ (18 files) ✅
├── Admissions/
│   └── Entities/ (7 placeholder files) ⚠️
├── Financial/
│   └── Entities/ (4 placeholder files) ⚠️
├── Assessment/
│   └── Entities/ (6 placeholder files) ⚠️
├── Learning/
│   └── Entities/ (2 placeholder files) ⚠️
├── Communication/
│   └── Entities/ (5 placeholder files) ⚠️
└── SASpecific/
    └── Entities/ (6 placeholder files) ⚠️
```

## Reference Documents

1. **Entity Creation Status**: `ENTITY_CREATION_STATUS.md`
   - Detailed progress tracking
   - Completion checklist

2. **Implementation Templates**: `REMAINING_ENTITIES_TEMPLATE.md`
   - Complete specifications for remaining 31 entities
   - Field definitions, patterns, and requirements

3. **Domain Model Reference**: `C:\Users\Rapudi Hlakudi\Desktop\PSMS\PSMS-Domain-Model-CORRECTED.md`
   - Complete domain model specification
   - Business rules and relationships

## Next Steps

To complete the remaining 31 entities:

1. Review completed Academic module entities as reference
2. Follow patterns documented in `REMAINING_ENTITIES_TEMPLATE.md`
3. Implement each entity with:
   - Correct namespaces and using statements
   - Proper base class and interfaces
   - All required fields and properties
   - Navigation properties
   - Constructors
   - XML documentation

## Example Entity Pattern

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.[ModuleName].Entities
{
    /// <summary>
    /// Entity description
    /// </summary>
    [Table("TableName")]
    public class EntityName : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxNameLength = 100;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Property description
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string PropertyName { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual ICollection<RelatedEntity> RelatedEntities { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected EntityName()
        {
            RelatedEntities = new HashSet<RelatedEntity>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public EntityName(Guid id, int? tenantId, string propertyName) : this()
        {
            Id = id;
            TenantId = tenantId;
            PropertyName = propertyName;
            IsDeleted = false;
        }
    }
}
```

## Quality Checklist

For each entity:
- [ ] Follows ASP.NET Boilerplate patterns (Abp.*, not Volo.Abp.*)
- [ ] Uses int? for TenantId
- [ ] Uses long for UserId references
- [ ] Has XML documentation
- [ ] Has proper data annotations
- [ ] Has both protected and public constructors
- [ ] Initializes collections in constructor
- [ ] Has navigation properties with [ForeignKey]
- [ ] Has constants for max lengths
- [ ] Follows naming conventions

## Contact

For questions or clarifications, refer to:
- ASP.NET Boilerplate documentation: https://aspnetboilerplate.com
- Project domain model: PSMS-Domain-Model-CORRECTED.md
