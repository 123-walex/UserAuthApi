using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
// basically telling efcore how to make my db 
namespace User_Authapi.Data
{
    public class UsersDbcontextFactory : IDesignTimeDbContextFactory<UsersDbcontext>
    {
        public UsersDbcontext CreateDbContext(string[] args)
        {
            // Load appsettings.json from the project directory
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Get the connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configure the DbContext with SQL Server
            var optionsBuilder = new DbContextOptionsBuilder<UsersDbcontext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new UsersDbcontext(optionsBuilder.Options);
        }
    }
}
