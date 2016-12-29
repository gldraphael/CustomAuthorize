using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CustomAuthorize.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomAuthorize.Data
{
    public class SampleData
    {
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider, bool createUsers = true)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                if (await db.Database.EnsureCreatedAsync())
                {
                    // No test data to insert
                    if (createUsers)
                    {
                        await CreateUser(serviceProvider, "DefaultUser", "DefaultPassword");
                        await CreateUser(serviceProvider, "DefaultAdmin", "DefaultPassword", new string[] { RoleConstants.Admin });
                        await CreateUser(serviceProvider, "DefaultMod", "DefaultPassword", new string[] { RoleConstants.Moderator });
                    }
                }
            }
        }

        /// <summary>
        /// Creates a store manager user who can manage the inventory.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static async Task CreateUser(IServiceProvider serviceProvider, string usernameKey, string passwordKey, string[] roles = null)
        {
            var env = serviceProvider.GetService<IHostingEnvironment>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            // Check if the user exists
            var user = await userManager.FindByNameAsync(configuration[usernameKey]);
            if (user == null)
            {
                // Create the user
                user = new ApplicationUser { UserName = configuration[usernameKey] };
                await userManager.CreateAsync(user, configuration[passwordKey]);

                // Add roles
                if(roles != null && roles.Length > 0)
                {
                    foreach(var r in roles)
                    {
                        await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, r));
                    }
                }
            }
        }

    }
}