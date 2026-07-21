using CodegateTest.Models;
using Microsoft.AspNetCore.Identity;

namespace CodegateTest.Utilites.DbIntialiaion
{
    public class DbIntializer :IDbIntializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbIntializer(UserManager<ApplicationUser> userManager , RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;


        }

        

        public async Task dbIntializer()
        {
            // Create Role ..
           await _roleManager.CreateAsync(new(SD.ADMIN_ROLE));   
           await _roleManager.CreateAsync(new(SD.STUDENT_ROLE));   
          
            
            // Create Admin
            await _userManager.CreateAsync(new ApplicationUser
            {
                Fname = "abdo" ,
                Lname = "osama",
                Email ="abdoosama01095160180@gmail.com",
                UserName = "Abdelrahman Osama",
                EmailConfirmed = true
            }, "SuperAdmin@123#");
           

            var Admin = await _userManager.FindByNameAsync("Abdelrahman Osama");
            if (Admin is not null)
            {
                await _userManager.AddToRoleAsync(Admin, SD.ADMIN_ROLE); 
            }

        }
    }
}
