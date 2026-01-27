using Abp.Zero.EntityFrameworkCore;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace psms.EntityFrameworkCore;

public class psmsDbContext : AbpZeroDbContext<Tenant, Role, User, psmsDbContext>
{
    /* Define a DbSet for each entity of the application */

    public psmsDbContext(DbContextOptions<psmsDbContext> options)
        : base(options)
    {
    }
}
