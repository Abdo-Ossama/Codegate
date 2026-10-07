using CodegateTest.Repositories.IRepositories;
using CodegateTest.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodegateTest.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    public class InstructorsController : ControllerBase
    {
        private readonly IRepository<Instructor> _instructorRepository;
        private readonly IImageService _imageService;

        public InstructorsController(IRepository<Instructor> instructorRepository,
            IImageService imageService)
        {
            _instructorRepository = instructorRepository;
            _imageService = imageService;
        }

        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var instructors = await _instructorRepository.GetAsync(e => !e.IsDeleted);

            return Ok(instructors.Select(ToResponse));
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var instructor = await _instructorRepository.GetOneAsync(e => e.Id == id && !e.IsDeleted);
            if (instructor is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Instructor is Not Found"]
                });
            }
            return Ok(ToResponse(instructor));
        }




        [HttpPost]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Create(
       IFormFile? logo,
       [FromForm] InstructorCreateRequest instructorCreateRequest)
        {
            var instructor = instructorCreateRequest.Adapt<Instructor>();

            string? newImage = null;
            try
            {
                if (logo is not null)
                {
                    newImage = await _imageService.UploadImageAsync(logo, "instructors_img");
                    instructor.AvatarUrl = newImage;
                }
                await _instructorRepository.CreateAsync(instructor);
                if (await _instructorRepository.CommitAsync() <= 0)
                {
                    if (newImage is not null)
                        _imageService.DeleteImage(newImage, "instructors_img");
                    return SaveImageFailure();
                }
            }
            catch (ArgumentException exception)
            {
                if (newImage is not null)
                    _imageService.DeleteImage(newImage, "instructors_img");
                return BadRequest(new APIResponce { StatusCode = 400, Message = [exception.Message] });
            }
            catch
            {
                if (newImage is not null)
                    _imageService.DeleteImage(newImage, "instructors_img");
                throw;
            }

            return StatusCode(StatusCodes.Status201Created, new APIResponce
            {
                StatusCode = 201,
                Message = ["Instructor Created Successfully"]
            });
        }




        [HttpPut("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Update(
      int id,
      IFormFile? logo,
      [FromForm] InstructorUpdateRequest instructorUpdateRequest)
        {
            var instructor = await _instructorRepository.GetOneAsync(
                e => e.Id == id && !e.IsDeleted
            );

            if (instructor is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Instructor is Not Found"]
                });
            }

            instructor.FirstName =
                instructorUpdateRequest.FirstName ?? instructor.FirstName;

            instructor.LastName =
                instructorUpdateRequest.LastName ?? instructor.LastName;

            instructor.Title =
                instructorUpdateRequest.Title ?? instructor.Title;

            var oldImage = instructor.AvatarUrl;
            string? newImage = null;
            try
            {
                if (logo is not null)
                {
                    newImage = await _imageService.UploadImageAsync(logo, "instructors_img");
                    instructor.AvatarUrl = newImage;
                }
                _instructorRepository.Update(instructor);
                var saved = await _instructorRepository.CommitAsync();
                if (newImage is not null && saved <= 0)
                {
                    _imageService.DeleteImage(newImage, "instructors_img");
                    return SaveImageFailure();
                }
            }
            catch (ArgumentException exception)
            {
                if (newImage is not null)
                    _imageService.DeleteImage(newImage, "instructors_img");
                return BadRequest(new APIResponce { StatusCode = 400, Message = [exception.Message] });
            }
            catch
            {
                if (newImage is not null)
                    _imageService.DeleteImage(newImage, "instructors_img");
                throw;
            }

            if (newImage is not null && !string.IsNullOrEmpty(oldImage))
                _imageService.DeleteImage(oldImage, "instructors_img");

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Instructor Updated Successfully"]
            });
        }

        private object ToResponse(Instructor instructor) => new
        {
            instructor.Id,
            instructor.FirstName,
            instructor.LastName,
            instructor.Title,
            AvatarUrl = _imageService.GetImageUrl(instructor.AvatarUrl, "instructors_img"),
            instructor.CreatedAt,
            instructor.IsDeleted,
            instructor.CourseInstructors
        };

        private IActionResult SaveImageFailure() => StatusCode(500, new APIResponce
        {
            StatusCode = 500,
            Message = ["Failed to save the instructor. The previous image has been kept."]
        });

        [HttpDelete("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Delete(int id)
        {
            var instructor = await _instructorRepository.GetOneAsync(
                e => e.Id == id && !e.IsDeleted
            );

            if (instructor is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Instructor is Not Found"]
                });
            }

            instructor.IsDeleted = true;

            _instructorRepository.Update(instructor);

            await _instructorRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Instructor Deleted Successfully"]
            });
        }
    }
}
