using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services) //IServiceProvider — to kontener DI
        {
            //Tworzymy zakres (scope), bo RoleManager<>, UserManager<> i pod spodem DbContext mają zwykle lifetime Scoped.
            //Żeby je poprawnie rozwiązać poza kontrolerem, musimy utworzyć scope ręcznie.
            using var scope = services.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roles = new[] { "Admin", "Worker", "Client" };
            foreach (var r in roles)
                //sprawdza, czy rola już jest w bazie.
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            var adminEmail = "admin@local.com";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "Local",
                    RegisteredAt = DateTime.UtcNow
                };
                await userMgr.CreateAsync(admin, "Admin123!");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
