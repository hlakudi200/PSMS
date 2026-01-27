using psms.Configuration;
using psms.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace psms.EntityFrameworkCore;

/* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
public class psmsDbContextFactory : IDesignTimeDbContextFactory<psmsDbContext>
{
    public psmsDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<psmsDbContext>();

        /*
         You can provide an environmentName parameter to the AppConfigurations.Get method. 
         In this case, AppConfigurations will try to read appsettings.{environmentName}.json.
         Use Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") method or from string[] args to get environment if necessary.
         https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#args
         */
        var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

        psmsDbContextConfigurer.Configure(builder, configuration.GetConnectionString(psmsConsts.ConnectionStringName));

        return new psmsDbContext(builder.Options);
    }
}
