using CodegateTest.Repositories.IRepositories;
using CodegateTest.Services;
using CodegateTest.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IImageService _imageService;



        public CoursesController(IRepository<Course> courseRepository,
            IRepository<CourseInstructors> courseInstractorRepository,
            IRepository<Instructor> instructorRepository,
            IImageService imageService)

        {
            _courseRepository = courseRepository;
            _courseInstractorRepository = courseInstractorRepository;
            _instructorRepository = instructorRepository;
            _imageService = imageService;



        }
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int page = 1)
        {
            var courses = await _courseRepository.GetAsync(e => !e.IsDeleted,
       includes: [e => e.CourseInstructors,]

   );
            var courseInstructors = await _courseInstractorRepository.GetAsync(includes: [e => e.Instructor]);

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
                items = cousreQuery.Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Slug,
                    e.Price,
                    e.Description,
                    e.IsActive,
                    e.CoverImageUrl,

                    Instructors = e.CourseInstructors
            .Select(e =>
                $"{e.Instructor.FirstName} {e.Instructor.LastName}")
            .ToList()
                }).ToList(),
                pageSize = pageSize,
                totalPages = totalPages,
                totalCourses = totalCourses


            });


        }


        [HttpGet("{id}")]
        [AllowAnonymous]
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

            return Ok(new CoursesResponce
            {
                items = new
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
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> CreateCourse(
    [FromForm] CreateCourseRequest createCourseRequest)
        {
            var course = new Course
            {
                Name = createCourseRequest.Name,
                Slug = createCourseRequest.Slug,
                Price = createCourseRequest.Price,
                Description = createCourseRequest.Description,
                CoverImageUrl = await _imageService.UploadImageAsync(
                    createCourseRequest.CoverImage,
                    "courses_img"
                ),
                CourseInstructors = createCourseRequest.InstructorIds
                    .Select(id => new CourseInstructors
                    {
                        InstructorId = id
                    })
                    .ToList()
            };

            await _courseRepository.CreateAsync(course);
            await _courseRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Course Created Successfully"]
            });
        }


        [HttpPut("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Update(
      int id,
      [FromForm] CourseUpdateRequest courseUpdateRequest)
        {
            var courseInDb = await _courseRepository.GetOneAsync(
                e => e.Id == id,
                includes: [e => e.CourseInstructors]
            );

            if (courseInDb is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Course Not Found"]
                });
            }

            // Update Course Image
            if (courseUpdateRequest.CoverImg is not null)
            {

                if (!string.IsNullOrEmpty(courseInDb.CoverImageUrl))
                {
                    _imageService.DeleteImage(
                        courseInDb.CoverImageUrl,
                        "courses_img"
                    );
                }

                courseInDb.CoverImageUrl = await _imageService.UploadImageAsync(
                    courseUpdateRequest.CoverImg,
                    "courses_img"
                );
            }

            // Update Course Data
            courseInDb.Name =
                courseUpdateRequest.Name ?? courseInDb.Name;

            courseInDb.Slug =
                courseUpdateRequest.Slug ?? courseInDb.Slug;

            courseInDb.Price =
                courseUpdateRequest.Price ?? courseInDb.Price;

            courseInDb.Description =
                courseUpdateRequest.Description ?? courseInDb.Description;

            courseInDb.IsActive =
                courseUpdateRequest.IsActive ?? courseInDb.IsActive;

            // Update Instructors
            if (courseUpdateRequest.InstructorIds is not null)
            {
                courseInDb.CourseInstructors.Clear();

                foreach (var instructorId in courseUpdateRequest.InstructorIds)
                {
                    courseInDb.CourseInstructors.Add(new CourseInstructors
                    {
                        CourseId = courseInDb.Id,
                        InstructorId = instructorId
                    });
                }
            }

            _courseRepository.Update(courseInDb);

            await _courseRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Course Updated Successfully"]
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetOneAsync(
                e => e.Id == id
            );

            if (course is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Course Not Found"]
                });
            }

            course.IsDeleted = true;

            await _courseRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Course Deleted Successfully"]
            });
        }
    }


}

