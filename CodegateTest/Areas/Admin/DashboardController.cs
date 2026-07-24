using CodegateTest.Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodegateTest.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_ROLE)]
 
    public class DashboardController : ControllerBase
        
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Instructor> _instructorRepository;
        private readonly IRepository<Contact>  _contactRepository;




        public DashboardController(UserManager<ApplicationUser> userManager, 
            IRepository<Course> courseRepository,
            IRepository<Instructor> instructorRepository,
            IRepository<Contact> contactRepository)
        {
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _userManager = userManager;
            _contactRepository = contactRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
          var usersCount= await _userManager.Users.CountAsync();

            var studentCount = (await _userManager.GetUsersInRoleAsync(SD.STUDENT_ROLE)).Count;
            var instructors = await _instructorRepository.GetAsync(e=>!e.IsDeleted);

            var instructorsCount = instructors.Count();

            var courses = await _courseRepository.GetAsync(e => !e.IsDeleted);
            var coursesCount = courses.Count();

            var lastCourses = (await _courseRepository
      .GetAsync(e => !e.IsDeleted))
      .OrderByDescending(e => e.CreatedAt)
      .Take(3)
      .ToList();

            var lastUsers = await _userManager.Users
       .OrderByDescending(e => e.CreatedAt)
       .Take(3)
       .Select(e => new
       {
           e.Id,
           e.UserName,
           e.Email
       })
       .ToListAsync();

            var lastContactMessages = (await _contactRepository.GetAsync())
       .OrderByDescending(e => e.CreatedAt)
       .Take(3)
       .ToList();



            return Ok(new
            {
                usersCount,
                studentCount,
                instructorsCount,
                coursesCount,
                lastCourses,
                lastUsers,
                lastContactMessages


            });
        }

    }
}