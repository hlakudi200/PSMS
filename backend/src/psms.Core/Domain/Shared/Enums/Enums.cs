namespace psms.Domain.Shared.Enums
{
    // ==================== Academic Module Enums ====================

    /// <summary>
    /// Gender options
    /// </summary>
    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }

    /// <summary>
    /// Student enrollment status
    /// </summary>
    public enum StudentStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Expelled = 4,
        Graduated = 5,
        Transferred = 6,
        Withdrawn = 7
    }

    /// <summary>
    /// General enrollment status for services (transport, after-care, etc.)
    /// </summary>
    public enum EnrollmentStatus
    {
        Active = 1,
        Suspended = 2,
        Terminated = 3,
        Pending = 4
    }

    /// <summary>
    /// Relationship type for parents/guardians
    /// </summary>
    public enum RelationshipType
    {
        Father = 1,
        Mother = 2,
        Guardian = 3,
        Stepfather = 4,
        Stepmother = 5,
        Grandparent = 6,
        Sibling = 7,
        Other = 8
    }

    /// <summary>
    /// Attendance status
    /// </summary>
    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        Excused = 4,
        SickLeave = 5,
        Holiday = 6
    }

    /// <summary>
    /// CAPS Achievement Levels (1-7 scale used in South African schools)
    /// </summary>
    public enum CapsAchievementLevel
    {
        Level1 = 1,  // 0-29% Not Achieved
        Level2 = 2,  // 30-39% Elementary Achievement
        Level3 = 3,  // 40-49% Moderate Achievement
        Level4 = 4,  // 50-59% Adequate Achievement
        Level5 = 5,  // 60-69% Substantial Achievement
        Level6 = 6,  // 70-79% Meritorious Achievement
        Level7 = 7   // 80-100% Outstanding Achievement
    }

    /// <summary>
    /// POPIA consent status
    /// </summary>
    public enum ConsentStatus
    {
        Pending = 1,
        Granted = 2,
        Denied = 3,
        Withdrawn = 4,
        Expired = 5
    }

    // ==================== Admissions Module Enums ====================

    /// <summary>
    /// Application status in the admissions process
    /// </summary>
    public enum ApplicationStatus
    {
        Draft = 1,
        Submitted = 2,
        UnderReview = 3,
        DocumentsPending = 4,
        InterviewScheduled = 5,
        AssessmentScheduled = 6,
        PendingDecision = 7,
        Approved = 8,
        Rejected = 9,
        Waitlisted = 10,
        Enrolled = 11,
        Withdrawn = 12,
        Expired = 13
    }

    /// <summary>
    /// Admission decision
    /// </summary>
    public enum AdmissionDecision
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3,
        Waitlisted = 4,
        ConditionalAcceptance = 5
    }

    /// <summary>
    /// Document categories for applications
    /// </summary>
    public enum DocumentCategory
    {
        BirthCertificate = 1,
        IdDocument = 2,
        PreviousSchoolReport = 3,
        TransferLetter = 4,
        ImmunizationRecord = 5,
        MedicalReport = 6,
        ProofOfResidence = 7,
        ParentIdDocument = 8,
        Photo = 9,
        Other = 10
    }

    /// <summary>
    /// Interview status
    /// </summary>
    public enum InterviewStatus
    {
        Scheduled = 1,
        Rescheduled = 2,
        Completed = 3,
        Cancelled = 4,
        NoShow = 5
    }

    /// <summary>
    /// Assessment type for admissions
    /// </summary>
    public enum AssessmentType
    {
        Placement = 1,
        Diagnostic = 2,
        Readiness = 3,
        LanguageProficiency = 4,
        Mathematics = 5,
        General = 6
    }

    /// <summary>
    /// Waitlist status
    /// </summary>
    public enum WaitlistStatus
    {
        Active = 1,
        Offered = 2,
        Accepted = 3,
        Declined = 4,
        Expired = 5,
        Withdrawn = 6
    }

    // ==================== Financial Module Enums ====================

    /// <summary>
    /// Payment status
    /// </summary>
    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Refunded = 5,
        PartiallyPaid = 6,
        Overdue = 7
    }

    /// <summary>
    /// South African payment methods
    /// </summary>
    public enum SouthAfricanPaymentMethod
    {
        EFT = 1,              // Electronic Funds Transfer
        DebitOrder = 2,       // Recurring debit order
        CreditCard = 3,
        DebitCard = 4,
        Cash = 5,
        Cheque = 6,
        PayFast = 7,          // SA payment gateway
        SnapScan = 8,         // Mobile payment
        Zapper = 9,           // Mobile payment
        Ozow = 10,            // Instant EFT
        BankDeposit = 11
    }

    /// <summary>
    /// Fee types specific to South African schools
    /// </summary>
    public enum SouthAfricanFeeType
    {
        Tuition = 1,
        Registration = 2,
        Stationery = 3,
        Uniform = 4,
        Transport = 5,
        AfterCare = 6,
        Extramural = 7,
        Camp = 8,
        Excursion = 9,
        Technology = 10,
        Library = 11,
        Laboratory = 12,
        SportLevy = 13,
        BuildingFund = 14,
        Other = 15
    }

    /// <summary>
    /// Fee types
    /// </summary>
    public enum FeeType
    {
        Tuition = 1,
        Registration = 2,
        Application = 3,
        Transport = 4,
        AfterCare = 5,
        Extramural = 6,
        Uniform = 7,
        Stationery = 8,
        Other = 9
    }

    /// <summary>
    /// Payment frequency
    /// </summary>
    public enum PaymentFrequency
    {
        Once = 1,
        Monthly = 2,
        Quarterly = 3,
        PerTerm = 4,
        BiAnnually = 5,
        Annually = 6
    }

    /// <summary>
    /// Fee status
    /// </summary>
    public enum FeeStatus
    {
        Pending = 1,
        Paid = 2,
        PartiallyPaid = 3,
        Overdue = 4,
        Waived = 5,
        Cancelled = 6
    }

    // ==================== Assessment Module Enums ====================

    /// <summary>
    /// CAPS assessment categories
    /// </summary>
    public enum CapsAssessmentCategory
    {
        FormalTest = 1,
        FormalExam = 2,
        Assignment = 3,
        Project = 4,
        Practical = 5,
        Oral = 6,
        Investigation = 7,
        ControlledTest = 8,
        CaseStudy = 9
    }

    /// <summary>
    /// Mark/grade status
    /// </summary>
    public enum MarkStatus
    {
        Pending = 1,
        Completed = 2,
        Absent = 3,
        Exempted = 4,
        Incomplete = 5
    }

    /// <summary>
    /// Report type
    /// </summary>
    public enum ReportType
    {
        Term1 = 1,
        Term2 = 2,
        Term3 = 3,
        Term4 = 4,
        MidYear = 5,
        YearEnd = 6,
        Progress = 7
    }

    /// <summary>
    /// Report status
    /// </summary>
    public enum ReportStatus
    {
        Draft = 1,
        Generated = 2,
        PendingApproval = 3,
        Approved = 4,
        Published = 5
    }

    /// <summary>
    /// Promotion decision
    /// </summary>
    public enum PromotionDecision
    {
        Promoted = 1,
        Retained = 2,
        ConditionalPromotion = 3,
        ProgressedWithSupport = 4
    }

    /// <summary>
    /// Question types for assessments
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice = 1,
        TrueFalse = 2,
        ShortAnswer = 3,
        LongAnswer = 4,
        Essay = 5,
        Matching = 6,
        FillInBlank = 7,
        Practical = 8
    }

    /// <summary>
    /// Cognitive levels (Bloom's Taxonomy)
    /// </summary>
    public enum CognitiveLevel
    {
        Remembering = 1,
        Understanding = 2,
        Applying = 3,
        Analyzing = 4,
        Evaluating = 5,
        Creating = 6
    }

    // ==================== Learning Module Enums ====================

    /// <summary>
    /// Learning material types
    /// </summary>
    public enum LearningMaterialType
    {
        Document = 1,
        Video = 2,
        Audio = 3,
        Presentation = 4,
        Worksheet = 5,
        ExternalLink = 6,
        Image = 7,
        Interactive = 8
    }

    /// <summary>
    /// Online lesson platforms
    /// </summary>
    public enum OnlinePlatform
    {
        Zoom = 1,
        MicrosoftTeams = 2,
        GoogleMeet = 3,
        BigBlueButton = 4,
        WebEx = 5,
        Custom = 6
    }

    /// <summary>
    /// Online lesson status
    /// </summary>
    public enum OnlineLessonStatus
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Rescheduled = 5
    }

    // ==================== Communication Module Enums ====================

    /// <summary>
    /// Announcement types
    /// </summary>
    public enum AnnouncementType
    {
        General = 1,
        Academic = 2,
        Sports = 3,
        Event = 4,
        Emergency = 5,
        Holiday = 6,
        Administrative = 7
    }

    /// <summary>
    /// Announcement priority
    /// </summary>
    public enum AnnouncementPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    /// <summary>
    /// Announcement audience
    /// </summary>
    public enum AnnouncementAudience
    {
        All = 1,
        Staff = 2,
        Teachers = 3,
        Parents = 4,
        Students = 5,
        Grade = 6,
        Class = 7
    }

    /// <summary>
    /// Notification types
    /// </summary>
    public enum NotificationType
    {
        General = 1,
        Academic = 2,
        Financial = 3,
        Attendance = 4,
        Assessment = 5,
        Communication = 6,
        System = 7
    }

    /// <summary>
    /// Notification priority
    /// </summary>
    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Shared document types
    /// </summary>
    public enum SharedDocumentType
    {
        Policy = 1,
        Form = 2,
        Newsletter = 3,
        Calendar = 4,
        Curriculum = 5,
        Handbook = 6,
        Template = 7,
        Other = 8
    }

    /// <summary>
    /// Document audience
    /// </summary>
    public enum DocumentAudience
    {
        Public = 1,
        Staff = 2,
        Teachers = 3,
        Parents = 4,
        Students = 5,
        All = 6
    }

    // ==================== SA Specific Module Enums ====================

    /// <summary>
    /// Transport types
    /// </summary>
    public enum TransportType
    {
        SchoolBus = 1,
        Minibus = 2,
        Kombi = 3,
        Private = 4
    }

    /// <summary>
    /// Transport direction
    /// </summary>
    public enum TransportDirection
    {
        ToSchool = 1,
        FromSchool = 2,
        Both = 3
    }

    /// <summary>
    /// After-care types
    /// </summary>
    public enum AfterCareType
    {
        Standard = 1,
        Extended = 2,
        HolidayProgram = 3
    }

    /// <summary>
    /// Extramural categories
    /// </summary>
    public enum ExtramuralCategory
    {
        Sport = 1,
        Culture = 2,
        Academic = 3,
        Club = 4
    }

    /// <summary>
    /// Extramural activity types
    /// </summary>
    public enum ExtramuralType
    {
        Team = 1,
        Individual = 2,
        Group = 3
    }

    /// <summary>
    /// School seasons (for sports)
    /// </summary>
    public enum SchoolSeason
    {
        Summer = 1,
        Winter = 2,
        AllYear = 3
    }

    /// <summary>
    /// South African provinces
    /// </summary>
    public enum SouthAfricanProvince
    {
        EasternCape = 1,
        FreeState = 2,
        Gauteng = 3,
        KwaZuluNatal = 4,
        Limpopo = 5,
        Mpumalanga = 6,
        NorthWest = 7,
        NorthernCape = 8,
        WesternCape = 9
    }

    /// <summary>
    /// South African grade levels (R = Reception/Grade R, 1-12 = Grades 1-12)
    /// </summary>
    public enum SouthAfricanGradeLevel
    {
        GradeR = 0,
        Grade1 = 1,
        Grade2 = 2,
        Grade3 = 3,
        Grade4 = 4,
        Grade5 = 5,
        Grade6 = 6,
        Grade7 = 7,
        Grade8 = 8,
        Grade9 = 9,
        Grade10 = 10,
        Grade11 = 11,
        Grade12 = 12
    }

    /// <summary>
    /// South African school phases based on CAPS curriculum
    /// </summary>
    public enum SouthAfricanSchoolPhase
    {
        /// <summary>Foundation Phase: Grade R-3</summary>
        Foundation = 1,
        /// <summary>Intermediate Phase: Grade 4-6</summary>
        Intermediate = 2,
        /// <summary>Senior Phase: Grade 7-9</summary>
        Senior = 3,
        /// <summary>FET Phase (Further Education and Training): Grade 10-12</summary>
        FET = 4
    }

    /// <summary>
    /// South African term numbers (4-term academic year system)
    /// </summary>
    public enum SouthAfricanTermNumber
    {
        Term1 = 1,  // Jan-Mar
        Term2 = 2,  // Apr-Jun
        Term3 = 3,  // Jul-Sep
        Term4 = 4   // Oct-Dec
    }

    /// <summary>
    /// Types of calendar events in a term
    /// </summary>
    public enum EventType
    {
        PublicHoliday = 1,
        SchoolHoliday = 2,
        Examination = 3,
        Assessment = 4,
        SportEvent = 5,
        CulturalEvent = 6,
        ParentMeeting = 7,
        StaffMeeting = 8,
        Other = 99
    }
}
