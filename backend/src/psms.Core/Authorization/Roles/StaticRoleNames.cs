namespace psms.Authorization.Roles;

/// <summary>
/// Static role names for the PSMS (Private School Management System).
/// See PSMS-Permissions-Matrix.md for role-permission mappings.
/// </summary>
public static class StaticRoleNames
{
    /// <summary>
    /// Host-level roles (platform administrators)
    /// </summary>
    public static class Host
    {
        /// <summary>
        /// Platform administrator with full access to all tenants
        /// </summary>
        public const string Admin = "Admin";
    }

    /// <summary>
    /// Tenant-level roles (school-specific)
    /// </summary>
    public static class Tenants
    {
        /// <summary>
        /// School IT administrator with full tenant access
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// School principal with all academic/admission decision authority
        /// </summary>
        public const string Principal = "Principal";

        /// <summary>
        /// Vice Principal - assists principal with limited decision authority
        /// </summary>
        public const string VicePrincipal = "VicePrincipal";

        /// <summary>
        /// Head of Department - manages specific subject area/grade
        /// </summary>
        public const string HOD = "HOD";

        /// <summary>
        /// Admissions Officer - manages admission applications
        /// </summary>
        public const string AdmissionsOfficer = "AdmissionsOfficer";

        /// <summary>
        /// Finance Manager - manages fees, payments, financial reporting
        /// </summary>
        public const string Finance = "Finance";

        /// <summary>
        /// Teacher - manages assigned classes and subjects
        /// </summary>
        public const string Teacher = "Teacher";

        /// <summary>
        /// Parent/Guardian - views child's information, makes payments
        /// </summary>
        public const string Parent = "Parent";

        /// <summary>
        /// Student - views own information, submits work
        /// </summary>
        public const string Student = "Student";

        /// <summary>
        /// Prospective Parent - submits and tracks admission applications (public portal)
        /// </summary>
        public const string Applicant = "Applicant";
    }
}
