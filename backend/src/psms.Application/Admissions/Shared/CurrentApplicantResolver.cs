using Abp.Authorization;
using Abp.Dependency;
using Abp.Runtime.Session;
using psms.Authorization;
using System.Threading.Tasks;

namespace psms.Admissions.Shared;

/// <summary>
/// Whether the signed-in user is a prospective parent rather than school staff.
/// <para>
/// A parent applying to the school holds <c>Admissions.Applications.View</c> so
/// they can read their own application. Nothing checked that it <i>was</i>
/// theirs: <c>Get</c> took an id and <c>GetByApplicationNumber</c> took a
/// number that runs in sequence — APP-003-2026-00011 — so one applicant could
/// read another family's application by counting. A child's name, date of
/// birth, ID number and both parents' contact details.
/// </para>
/// <para>
/// Staff are told apart by <c>Admissions.Applications.ViewAll</c>, which is the
/// permission that means "every application at this school" and which no
/// applicant holds. Anyone without it sees only what they created.
/// </para>
/// </summary>
public interface ICurrentApplicantResolver : ITransientDependency
{
    /// <summary>
    /// The user id whose applications the caller may read, or null when they
    /// may read all of them.
    /// </summary>
    Task<long?> GetOwnApplicationsOnlyForAsync();
}

public class CurrentApplicantResolver : ICurrentApplicantResolver
{
    private readonly IAbpSession _session;
    private readonly IPermissionChecker _permissionChecker;

    public CurrentApplicantResolver(IAbpSession session, IPermissionChecker permissionChecker)
    {
        _session = session;
        _permissionChecker = permissionChecker;
    }

    public async Task<long?> GetOwnApplicationsOnlyForAsync()
    {
        var userId = _session.UserId;
        if (!userId.HasValue)
            return null;

        // The permission that means "every application at this school".
        if (await _permissionChecker.IsGrantedAsync(PermissionNames.Admissions_Applications_ViewAll))
            return null;

        return userId.Value;
    }
}
