using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.Runtime.Session;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Domain.Academic.Entities;

namespace psms.Academic.Students;

/// <summary>
/// Provisions the AbpUser login account for a student and links it back onto
/// the entity (LC-07). Shared so EVERY student-creation path — the manual
/// StudentAppService and the admissions EnrollmentAppService — produces a
/// student with exactly one linked login.
/// </summary>
public interface IStudentLoginProvisioner : ITransientDependency
{
    Task<StudentLoginResult> ProvisionAsync(Student student);
}

/// <summary>One-time credentials returned to the caller right after provisioning.</summary>
public class StudentLoginResult
{
    public string UserName { get; set; }
    public string TemporaryPassword { get; set; }
}

public class StudentLoginProvisioner : IStudentLoginProvisioner
{
    private readonly UserManager _userManager;
    private readonly ILocalizationManager _localizationManager;
    private readonly IAbpSession _abpSession;

    public StudentLoginProvisioner(
        UserManager userManager,
        ILocalizationManager localizationManager,
        IAbpSession abpSession)
    {
        _userManager = userManager;
        _localizationManager = localizationManager;
        _abpSession = abpSession;
    }

    /// <summary>
    /// Creates the login (Student role) and links it to <paramref name="student"/>.
    /// Runs in the caller's unit of work, so a failure here rolls back the whole
    /// student create (no orphaned user, no student without a login). Returns the
    /// username + one-time temporary password.
    /// </summary>
    public async Task<StudentLoginResult> ProvisionAsync(Student student)
    {
        await _userManager.InitializeOptionsAsync(_abpSession.TenantId);

        var userName = SanitizeUserName(student.AdmissionNumber);
        // Students rarely have their own email; synthesise a unique, valid one
        // from the (tenant-unique) admission number when absent.
        var emailLocalPart = new string((student.AdmissionNumber ?? string.Empty)
            .ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        var email = !string.IsNullOrWhiteSpace(student.Email)
            ? student.Email.Trim()
            : $"{emailLocalPart}@students.psms.local";
        var tempPassword = GenerateTemporaryPassword();

        var user = new User
        {
            TenantId = _abpSession.TenantId,
            UserName = userName,
            Name = student.FirstName,
            Surname = student.LastName,
            EmailAddress = email,
            IsActive = true,
            IsEmailConfirmed = true,
        };

        (await _userManager.CreateAsync(user, tempPassword)).CheckErrors(_localizationManager);
        (await _userManager.SetRolesAsync(user, new[] { StaticRoleNames.Tenants.Student }))
            .CheckErrors(_localizationManager);

        student.LinkUser(user.Id);
        return new StudentLoginResult { UserName = userName, TemporaryPassword = tempPassword };
    }

    /// <summary>
    /// Maps a (free-text) admission number to a valid ABP username. ABP's default
    /// AllowedUserNameCharacters is letters, digits and -._@+, so anything else
    /// (spaces, '/', etc.) is replaced with '-' rather than failing the create.
    /// </summary>
    private static string SanitizeUserName(string admissionNumber)
    {
        var raw = (admissionNumber ?? string.Empty).Trim();
        var mapped = raw.Select(c =>
            char.IsLetterOrDigit(c) || "-._@+".IndexOf(c) >= 0 ? c : '-').ToArray();
        var cleaned = new string(mapped).Trim('-');
        return string.IsNullOrEmpty(cleaned)
            ? "student-" + Guid.NewGuid().ToString("N").Substring(0, 12)
            : cleaned;
    }

    /// <summary>
    /// Temporary password guaranteed to satisfy the usual complexity rules
    /// (upper + lower + digit + symbol, length 10), generated with a CSPRNG.
    /// </summary>
    private static string GenerateTemporaryPassword()
    {
        const string lower = "abcdefghijkmnpqrstuvwxyz"; // no l/o (ambiguous)
        const string digits = "23456789";                // no 0/1
        string Pick(string set, int n) =>
            new string(Enumerable.Range(0, n)
                .Select(_ => set[RandomNumberGenerator.GetInt32(set.Length)]).ToArray());
        return "S" + Pick(lower, 4) + Pick(digits, 3) + "#" + Pick(lower, 1);
    }
}
