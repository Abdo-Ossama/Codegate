using CodegateTest.Repositories.IRepositories;
using Mapster;
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

        public InstructorsController(IRepository<Instructor> instructorRepository)
        {
            _instructorRepository = instructorRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
         var instructors = await _instructorRepository.GetAsync(e => !e.IsDeleted);

            return Ok(instructors);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
         var instructor = await _instructorRepository.GetOneAsync(e=>e.Id == id );
            if(instructor is null)
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
        public async Task<IActionResult> Create(IFormFile logo , InstructorCreateRequest instructorCreateRequest)
        {
       var instructor = instructorCreateRequest.Adapt<Instructor>();
            if (logo is not null && logo.Length > 0)
            {
              
               var fileName =
                    Guid.NewGuid().ToString().Substring(0, 7) +
                    DateTime.UtcNow.ToString("yyyy-MM-dd") +
                    Path.GetExtension(logo.FileName);

                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "img",
                    "instructors_img"
                );

                Directory.CreateDirectory(folderPath);

                var filePath =
                    Path.Combine(folderPath, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await logo.CopyToAsync(stream);
                }

                instructor.AvatarUrl = fileName;
            }

            await _instructorRepository.CreateAsync(instructor);
            await _instructorRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 201,
                Message = ["Insructor Created Successfully "]
            });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    int id,
    IFormFile? logo,
    InstructorUpdateRequest instructorUpdateRequest)
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
                instructorUpdateRequest.FirstName
                ?? instructor.FirstName;

            instructor.LastName =
                instructorUpdateRequest.LastName
                ?? instructor.LastName;

            instructor.Title =
                instructorUpdateRequest.Title
                ?? instructor.Title;


            // Update Logo
            if (logo is not null && logo.Length > 0)
            {
                var fileName =
                    Guid.NewGuid().ToString().Substring(0, 7) +
                    DateTime.UtcNow.ToString("yyyy-MM-dd") +
                    Path.GetExtension(logo.FileName);

                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "img",
                    "instructors_img"
                );

                Directory.CreateDirectory(folderPath);

                var filePath =
                    Path.Combine(folderPath, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await logo.CopyToAsync(stream);
                }


                // Delete old logo
                if (!string.IsNullOrEmpty(instructor.AvatarUrl))
                {
                    var oldLogoPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "img",
                        "instructors_img",
                        instructor.AvatarUrl
                    );

                    if (System.IO.File.Exists(oldLogoPath))
                    {
                        System.IO.File.Delete(oldLogoPath);
                    }
                }

                instructor.AvatarUrl = fileName;
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
