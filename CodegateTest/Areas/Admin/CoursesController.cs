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
    [Route("api/[controller]")]
    [ApiController]

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
                .OrderByDescending(course => course.CreatedAt)
                .ThenBy(course => course.Id)
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
                    CoverImageUrl = _imageService.GetImageUrl(e.CoverImageUrl, "courses_img"),

                    Instructors = e.CourseInstructors
            .Where(link => !link.Instructor.IsDeleted)
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
                e => e.Id == id && !e.IsDeleted
            );

            if (course is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Course Not Found"]
                });
            }

            var courseInstructors =
                await _courseInstractorRepository.GetAsync(
                    e => e.CourseId == id && !e.Instructor.IsDeleted,
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
                    CoverImageUrl = _imageService.GetImageUrl(course.CoverImageUrl, "courses_img"),

                    Instructors = courseInstructors
    .Select(e => new
    {
        Name = $"{e.Instructor.FirstName} {e.Instructor.LastName}",
        AvatarUrl = _imageService.GetImageUrl(e.Instructor.AvatarUrl, "instructors_img")
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
            var instructorError = await ValidateInstructorIdsAsync(createCourseRequest.InstructorIds);
            if (instructorError is not null)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = 400,
                    Message = [instructorError]
                });
            }

            string image;
            try
            {
                image = await _imageService.UploadImageAsync(createCourseRequest.CoverImage, "courses_img");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new APIResponce { StatusCode = 400, Message = [exception.Message] });
            }

            try
            {
                var course = new Course
                {
                    Name = createCourseRequest.Name,
                    Slug = createCourseRequest.Slug,
                    Price = createCourseRequest.Price,
                    Description = createCourseRequest.Description,
                    CoverImageUrl = image,
                    CourseInstructors = createCourseRequest.InstructorIds
                        .Select(id => new CourseInstructors
                        {
                            InstructorId = id
                        })
                        .ToList()
                };

                await _courseRepository.CreateAsync(course);
                if (await _courseRepository.CommitAsync() <= 0)
                {
                    _imageService.DeleteImage(image, "courses_img");
                    return SaveImageFailure();
                }
            }
            catch
            {
                _imageService.DeleteImage(image, "courses_img");
                throw;
            }

            return StatusCode(StatusCodes.Status201Created, new APIResponce
            {
                StatusCode = 201,
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
                e => e.Id == id && !e.IsDeleted,
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

            if (courseUpdateRequest.InstructorIds is not null)
            {
                var instructorError = await ValidateInstructorIdsAsync(courseUpdateRequest.InstructorIds);
                if (instructorError is not null)
                {
                    return BadRequest(new APIResponce
                    {
                        StatusCode = 400,
                        Message = [instructorError]
                    });
                }
            }

            var oldImage = courseInDb.CoverImageUrl;
            string? newImage = null;

            if (courseUpdateRequest.CoverImg is not null)
            {
                try
                {
                    newImage = await _imageService.UploadImageAsync(courseUpdateRequest.CoverImg, "courses_img");
                    courseInDb.CoverImageUrl = newImage;
                }
                catch (ArgumentException exception)
                {
                    return BadRequest(new APIResponce { StatusCode = 400, Message = [exception.Message] });
                }
            }

            try
            {
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
                    var requestedIds = courseUpdateRequest.InstructorIds.ToHashSet();
                    foreach (var link in courseInDb.CourseInstructors
                        .Where(link => !requestedIds.Contains(link.InstructorId)).ToList())
                    {
                        _courseInstractorRepository.Delete(link);
                        courseInDb.CourseInstructors.Remove(link);
                    }

                    var existingIds = courseInDb.CourseInstructors
                        .Select(link => link.InstructorId).ToHashSet();
                    foreach (var instructorId in requestedIds.Except(existingIds))
                    {
                        var link = new CourseInstructors
                        {
                            CourseId = courseInDb.Id,
                            InstructorId = instructorId
                        };
                        courseInDb.CourseInstructors.Add(link);
                        await _courseInstractorRepository.CreateAsync(link);
                    }
                }

                _courseRepository.Update(courseInDb);

                var saved = await _courseRepository.CommitAsync();
                if (newImage is not null && saved <= 0)
                {
                    _imageService.DeleteImage(newImage, "courses_img");
                    return SaveImageFailure();
                }
            }
            catch
            {
                if (newImage is not null)
                    _imageService.DeleteImage(newImage, "courses_img");
                throw;
            }

            if (newImage is not null && !string.IsNullOrEmpty(oldImage))
                _imageService.DeleteImage(oldImage, "courses_img");

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Course Updated Successfully"]
            });
        }

        private async Task<string?> ValidateInstructorIdsAsync(List<int>? instructorIds)
        {
            if (instructorIds is null || instructorIds.Count == 0)
                return "At least one instructor is required.";
            if (instructorIds.Any(id => id <= 0))
                return "Instructor IDs must be positive.";
            if (instructorIds.Distinct().Count() != instructorIds.Count)
                return "Duplicate instructor IDs are not allowed.";

            var instructors = await _instructorRepository.GetAsync(
                instructor => instructorIds.Contains(instructor.Id) && !instructor.IsDeleted,
                tracked: false);
            return instructors.Count() == instructorIds.Count
                ? null
                : "One or more instructors do not exist or have been deleted.";
        }

        private IActionResult SaveImageFailure() => StatusCode(500, new APIResponce
        {
            StatusCode = 500,
            Message = ["Failed to save the course. The previous image has been kept."]
        });

        [HttpDelete("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetOneAsync(
                e => e.Id == id && !e.IsDeleted
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

