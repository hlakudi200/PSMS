using Abp.Authorization;
using psms.Authorization.Roles;
using psms.Authorization.Users;

namespace psms.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
