using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace FitnessCenterManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Kullanýcý Yöneticisi ve Rol Yöneticisi servislerini çaðýrýyoruz
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. Roller Var mý? Yoksa oluþtur.
            await CreateRoleAsync(roleManager, "Admin");
            await CreateRoleAsync(roleManager, "Member");
            await CreateRoleAsync(roleManager, "Trainer");

            // 2. Admin Kullanýcýsý Var mý? Yoksa oluþtur.
            var adminEmail = "g221210087@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Sistem Yöneticisi",
                    EmailConfirmed = true,
                    BirthDate = DateTime.Now.AddYears(-25)
                };

                // Þifre: sau (Proje isterindeki gibi)
                var result = await userManager.CreateAsync(newAdmin, "sau");

                if (result.Succeeded)
                {
                    // Admin rolünü ata
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}