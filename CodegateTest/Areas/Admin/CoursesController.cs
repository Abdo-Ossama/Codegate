using CodegateTest.Repositories.IRepositories;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodegateTest.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Instructor> _instructorRepository;
        private readonly IRepository<CourseInstructors> _courseInstractorRepository;

        public CoursesController(IRepository<Course> courseRepository ,
            IRepository<CourseInstructors> courseInstractorRepository)
        {
            _courseRepository = courseRepository;
            _courseInstractorRepository = courseInstractorRepository;
        }
        [HttpGet("")]
        public async Task<IActionResult> GetAll(int page = 1 )
        {
            var courses = await _courseRepository.GetAsync(
       includes: [e=>e.CourseInstructors , ]
           
   );
            var courseInstructors = await _courseInstractorRepository.GetAsync(includes:[e => e.Instructor]);
  
            var totalCourses = courses.Count();
            int pageSize = 5;

            if (page <= 0)
                page = 1;
            var totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);

            var cousreQuery = courses
     .Skip((page - 1) * pageSize)
     .Take(pageSize);
     

            return Ok(new CoursesResponce()
            {
                items = cousreQuery.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.Price,
                    c.Description,
                    c.IsActive,
                    c.CoverImageUrl,

                    Instructors = c.CourseInstructors
            .Select(e =>
                $"{e.Instructor.FirstName} {e.Instructor.LastName}")
            .ToList()
                }).ToList(),
                pageSize = pageSize,
                totalPages = totalPages ,
                 totalCourses =totalCourses 


            });


        }


      [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var course = await _courseRepository.GetOneAsync(
                e => e.Id == id
            );

            if (course is null)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = 400,
                    Message = ["Course Not Found"]
                });
            }

            var courseInstructors =
                await _courseInstractorRepository.GetAsync(
                    e => e.CourseId == id,
                    includes: [e => e.Instructor]
                );

            return Ok(new CourseResponce
            {
                item = new
                {
                    course.Id,
                    course.Name,
                    course.Slug,
                    course.Price,
                    course.Description,
                    course.IsActive,
                    course.CoverImageUrl,

                    Instructors = courseInstructors
    .Select(e => new
    {
        Name = $"{e.Instructor.FirstName} {e.Instructor.LastName}",
        AvatarUrl = e.Instructor.AvatarUrl
    })
    .ToList()

                }
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile Img,
            [FromForm] CourseCreateRequest courseCreateRequest)
        {
            if (Img is null || Img.Length == 0)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = 400,
                    Message = ["Course image is required."]
                });
            }

            var instructors = await _instructorRepository.GetAsync(
                e => courseCreateRequest.InstructorIds.Contains(e.Id)
            );

            if (instructors.Count() != courseCreateRequest.InstructorIds.Count)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = 400,
                    Message = ["One or more instructors not found."]
                });
            }

            var course = courseCreateRequest.Adapt<Course>();

            // Add instructor to the course from the relation
            foreach (var instructor in instructors)
            {
                course.CourseInstructors.Add(new CourseInstructors
                {
                    InstructorId = instructor.Id
                });
            }

            await _courseRepository.CreateAsync(course);

            await _courseRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Course Created Successfully"]
            });
        }
    }


    }

