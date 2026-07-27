
using CodegateTest.Models;
using CodegateTest.Repositories.IRepositories;
using CodegateTest.Services;
using CodegateTest.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using LoginRequest = CodegateTest.DTOs.Requests.LoginRequest;
using RegisterRequest = CodegateTest.DTOs.Requests.RegisterRequest;
using ResetPasswordRequest = CodegateTest.DTOs.Requests.ResetPasswordRequest;
namespace CodegateTest.Areas.Identity
{

    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.IDENTITY_AREA)]

    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManger;
        private readonly IAccountService _accountService;
        private readonly IJWTHandler _jWTHandler;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        public AccountsController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            IJWTHandler jWTHandler,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository

            )
        {

            _userManager = userManager;
            _signInManger = signInManager;
            _accountService = accountService;
            _jWTHandler = jWTHandler;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var user = registerRequest.Adapt<ApplicationUser>();

            var seed = Uri.EscapeDataString($"{user.Fname}-{user.Lname}");
            user.ProfileImageUrl =
                $"https://api.dicebear.com/10.x/initials/svg?seed={seed}";

            var result = await _userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, SD.STUDENT_ROLE);

            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmLink = Url.Action(
                "Confirm",
                "Accounts",
                new
                {
                    area = SD.IDENTITY_AREA,
                    token,
                    userId = user.Id
                },
                Request.Scheme);

            if (string.IsNullOrEmpty(confirmLink))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        Message = ["Failed to generate confirmation link"]
                    });
            }

            await _accountService.sendEmailAsync(
                EmailType.ConfirmEmail,
                user,
                $"Click here to confirm your email: {confirmLink}");

            return StatusCode(StatusCodes.Status201Created,
                new APIResponce
                {
                    StatusCode = 201,
                    Message = ["Your registration completed successfully."]
                });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.UserName);

            if (user is null)
            {
                return Unauthorized(new APIResponce
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = ["Invalid username or password."]
                });
            }

            var result = await _signInManger.PasswordSignInAsync(
                user,
                loginRequest.Password,
                loginRequest.RememberMe,
                lockoutOnFailure: true);

            if (result.IsNotAllowed)
            {
                return Unauthorized(new APIResponce
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = ["Please confirm your email first."]
                });
            }

            if (result.IsLockedOut)
            {
                return Unauthorized(new APIResponce
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = ["Your account has been locked due to multiple failed login attempts."]
                });
            }

            if (!result.Succeeded)
            {
                return Unauthorized(new APIResponce
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = ["Invalid username or password."]
                });
            }

            var token = await _jWTHandler.GenerateTokenAsync(user.Id, user.Email!);

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = [$"Welcome, {user.UserName}"],
                Data = token
            });
        }


        [HttpGet("Confirm")]
        public async Task<IActionResult> Confirm(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["User not found."]
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                });
            }

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["Your email has been confirmed successfully."]
            });
        }

        [HttpGet("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["User not found."]
                });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ["Email is already confirmed."]
                });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmLink = Url.Action(
                "Confirm",
                "Accounts",
                new
                {
                    area = SD.IDENTITY_AREA,
                    token,
                    userId = user.Id
                },
                Request.Scheme);

            if (string.IsNullOrEmpty(confirmLink))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        Message = ["Failed to generate confirmation link."]
                    });
            }

            await _accountService.sendEmailAsync(
                EmailType.ConfirmEmail,
                user,
                $"Click here to confirm your email: {confirmLink}");

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["Confirmation email has been sent successfully."]
            });
        }



        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.Email);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["User not found."]
                });
            }

            var otpsCount = (await _applicationUserOTPRepository.GetAsync(
                e => e.ApplicationUserId == user.Id &&
                     e.CreatedAt >= DateTime.Now.AddHours(-24)))
                .Count();

            if (otpsCount >= 50)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ["You have exceeded the maximum number of OTP requests today."]
                });
            }

            var otp = new Random().Next(1000, 9999).ToString();

            await _applicationUserOTPRepository.CreateAsync(new ApplicationUserOTP
            {
                ApplicationUserId = user.Id,
                OTP = otp
            });

            await _applicationUserOTPRepository.CommitAsync();

            await _accountService.sendEmailAsync(
                EmailType.ForgetPassword,
                user,
                $"Your OTP is: {otp}. Please do not share it with anyone.");

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["OTP has been sent successfully."]
            });
        }

        [HttpPost("Validate-OTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateOTPRequest)
        {

            var user = await _userManager.FindByIdAsync(validateOTPRequest.ApplicationUserId);
            if (user is null) return NotFound();

            var otp = (await _applicationUserOTPRepository.GetAsync()).Where(e => e.IsValid && e.ApplicationUser.Id == user.Id)
                .OrderBy(e => e.Id).LastOrDefault();
            if (otp is null || otp.OTP != validateOTPRequest.OTP)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["Invalid OTP , Please Try Again !"]
                });
            }
            else
            {
                otp.IsUsed = true;
                _applicationUserOTPRepository.Update(otp);
                await _applicationUserOTPRepository.CommitAsync();
                return Ok(new APIResponce
                {
                    Message = ["OTP verified successfully"],

                });
            }

        }




        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByIdAsync(resetPasswordRequest.ApplicationUserId);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["User not found."]
                });
            }

            var userToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                userToken,
                resetPasswordRequest.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                });
            }

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["Password changed successfully."]
            });
        }
    }
}
