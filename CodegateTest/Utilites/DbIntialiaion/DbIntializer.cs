using CodegateTest.Models;
using Microsoft.AspNetCore.Identity;

namespace CodegateTest.Utilites.DbIntialiaion
{
    public class DbIntializer : IDbIntializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbIntializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task dbIntializer()
        {
            foreach (var roleName in new[] { SD.ADMIN_ROLE, SD.STUDENT_ROLE })
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    EnsureSucceeded(roleResult, $"Creating role '{roleName}'");
                }
            }

            const string adminEmail = "abdoosama01095160180@gmail.com";
            const string adminUserName = "abdo.Osama";


            var admin = await _userManager.FindByEmailAsync(adminEmail);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    Fname = "abdo",
                    Lname = "osama",
                    Email = adminEmail,
                    UserName = adminUserName,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(admin, "SuperAdmin@123#");
                EnsureSucceeded(createResult, "Creating the admin account");
            }
            else if (!admin.EmailConfirmed)
            {
                admin.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(admin);
                EnsureSucceeded(updateResult, "Confirming the admin email");
            }

            if (!await _userManager.IsInRoleAsync(admin, SD.ADMIN_ROLE))
            {
                var assignResult = await _userManager.AddToRoleAsync(admin, SD.ADMIN_ROLE);
                EnsureSucceeded(assignResult, "Assigning the admin role");
            }
        }

        private static void EnsureSucceeded(IdentityResult result, string operation)
        {
            if (result.Succeeded)
                return;

            var errors = string.Join("; ", result.Errors.Select(error =>
                $"{error.Code}: {error.Description}"));

            throw new InvalidOperationException($"{operation} failed: {errors}");
        }
    }
}
