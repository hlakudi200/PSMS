using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace psms.EntityFrameworkCore;

public static class psmsDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<psmsDbContext> builder, string connectionString)
    {
        builder.UseNpgsql(connectionString);
        //builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<psmsDbContext> builder, DbConnection connection)
    {
        //builder.UseSqlServer(connection);
        builder.UseNpgsql(connection);
    }
}
