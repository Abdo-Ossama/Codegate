//using CodegateTest.Services.IServices;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;

//namespace CodegateTest.Areas.Student
//{
//    [Route("api/[area]/[controller]")]
//    [ApiController]
//    [Area(SD.STUDENT_AREA)]
//    [Authorize]
//    public class ProfileController : ControllerBase
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly IImageService _imageService;

//        public ProfileController(UserManager<ApplicationUser> userManager,
//             IImageService imageService
//            )
//        {
//            _userManager = userManager;
//            _imageService = imageService;
//        }


//        [HttpGet]
//        public async Task<IActionResult> Get()
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId is null)
//            {
//                return Unauthorized();

//            }

//            var user = await _userManager.FindByIdAsync(userId);

//            if (user is null)
//            {
//                return NotFound(new APIResponce
//                {
//                    Message = ["User is not found"]
//                });
//            }

//            return Ok(new ProfileResponse
//            {

//                FullName = $"{user.Fname} {user.Lname}",
//                Email = user.Email!,
//                ProfileImage = user.ProfileImageUrl

//            });
//        }


//        [HttpPut]
//        public async Task<IActionResult> Update(
//     [FromForm] UpdateProfileRequest updateProfileRequest)
//        {
//            var userId = User.FindFirstValue(
//                ClaimTypes.NameIdentifier
//            );

//            if (userId is null)
//            {
//                return Unauthorized();
//            }

//            var user = await _userManager.FindByIdAsync(userId);

//            if (user is null)
//            {
//                return NotFound(new APIResponce
//                {
//                    Message = ["User is not found"]
//                });
//            }

//            user.Fname =
//                updateProfileRequest.Fname ?? user.Fname;

//            user.Lname =
//                updateProfileRequest.Lname ?? user.Lname;

//            if (updateProfileRequest.ProfileImage is not null)
//            {
//                var imageUrl =
//                    await _imageService.UploadImageAsync(
//                        updateProfileRequest.ProfileImage,
//                        "profiles"
//                    );

//                user.ProfileImageUrl = imageUrl;
//            }

//            var result = await _userManager.UpdateAsync(user);

//            if (!result.Succeeded)
//            {
//                return BadRequest(result.Errors);
//            }

//            return Ok(new APIResponce
//            {
//                StatusCode = 200,
//                Message = ["Profile updated successfully"]
//            });
//        }
//    }
//}
