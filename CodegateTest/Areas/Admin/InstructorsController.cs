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

            return Ok(instructors);
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var instructor = await _instructorRepository.GetOneAsync(e => e.Id == id);
            if (instructor is null)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Instructor is Not Found"]
                });
            }
            return Ok(instructor);
        }




        [HttpPost]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Create(
       IFormFile? logo,
       [FromForm] InstructorCreateRequest instructorCreateRequest)
        {
            var instructor = instructorCreateRequest.Adapt<Instructor>();

            if (logo is not null)
            {
                instructor.AvatarUrl = await _imageService.UploadImageAsync(
                    logo,
                    "instructors_img"
                );
            }

            await _instructorRepository.CreateAsync(instructor);
            await _instructorRepository.CommitAsync();

            return Ok(new APIResponce
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
                e => e.Id == id
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

            // Update Logo
            if (logo is not null)
            {
                if (!string.IsNullOrEmpty(instructor.AvatarUrl))
                {
                    _imageService.DeleteImage(
                        instructor.AvatarUrl,
                        "instructors_img"
                    );
                }

                instructor.AvatarUrl = await _imageService.UploadImageAsync(
                    logo,
                    "instructors_img"
                );
            }

            _instructorRepository.Update(instructor);

            await _instructorRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Instructor Updated Successfully"]
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Delete(int id)
        {
            var instructor = await _instructorRepository.GetOneAsync(
                e => e.Id == id
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
