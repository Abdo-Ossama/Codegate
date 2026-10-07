using CodegateTest.Repositories.IRepositories;
using CodegateTest.Services;
using CodegateTest.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
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
        private readonly ILogger<AccountsController> _logger;
        public AccountsController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            IJWTHandler jWTHandler,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository,
            ILogger<AccountsController> logger

            )
        {

            _userManager = userManager;
            _signInManger = signInManager;
            _accountService = accountService;
            _jWTHandler = jWTHandler;
            _applicationUserOTPRepository = applicationUserOTPRepository;
            _logger = logger;
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
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = result.Errors.Select(error => error.Description).ToArray()
                });
            }

            try
            {
                var roleResult = await _userManager.AddToRoleAsync(user, SD.STUDENT_ROLE);
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Student role assignment failed for user {UserId}: {Errors}",
                        user.Id, string.Join("; ", roleResult.Errors.Select(error => error.Description)));
                    return await RollbackRegistrationAsync(user);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Student role assignment failed for user {UserId}", user.Id);
                return await RollbackRegistrationAsync(user);
            }

            var emailSent = await TrySendConfirmationAsync(user, EmailType.ConfirmEmail);

            return StatusCode(StatusCodes.Status201Created,
                new APIResponce
                {
                    StatusCode = 201,
                    Message = emailSent
                        ? ["Your registration completed successfully. Please confirm your email."]
                        : ["Your account was created, but the confirmation email could not be sent. Please request a new confirmation email."],
                    Data = new { UserId = user.Id, EmailConfirmationSent = emailSent }
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
                    Message = ["The confirmation link is invalid or expired. Please request a new confirmation email."]
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

            if (!await TrySendConfirmationAsync(user, EmailType.ResendEmailConfirmation))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new APIResponce
                    {
                        StatusCode = StatusCodes.Status503ServiceUnavailable,
                        Message = ["Unable to send the confirmation email. Please try again later."]
                    });
            }

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["Confirmation email has been sent successfully."]
            });
        }



        private async Task<bool> TrySendConfirmationAsync(ApplicationUser user, EmailType emailType)
        {
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmLink = Url.Action("Confirm", "Accounts",
                    new { area = SD.IDENTITY_AREA, token, userId = user.Id }, Request.Scheme);
                if (string.IsNullOrEmpty(confirmLink))
                {
                    _logger.LogError("Failed to generate confirmation link for user {UserId}", user.Id);
                    return false;
                }

                await _accountService.sendEmailAsync(emailType, user,
                    $"Click here to confirm your email: {confirmLink}");
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to send confirmation email for user {UserId}", user.Id);
                return false;
            }
        }

        private async Task<IActionResult> RollbackRegistrationAsync(ApplicationUser user)
        {
            try
            {
                var rollback = await _userManager.DeleteAsync(user);
                if (rollback.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponce
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = ["Registration could not be completed. Please try again later."]
                    });
                }
                _logger.LogError("Failed to remove incomplete account {UserId}: {Errors}",
                    user.Id, string.Join("; ", rollback.Errors.Select(error => error.Description)));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to remove incomplete account {UserId}", user.Id);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponce
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = ["Your account may exist, but registration could not be completed. Please contact support before registering again."]
            });
        }

        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgetPassword(
     ForgetPasswordRequest forgetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(
                forgetPasswordRequest.Email);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["User not found."]
                });
            }

            var now = DateTime.UtcNow;

            var otpsCount = (await _applicationUserOTPRepository.GetAsync(
                e => e.ApplicationUserId == user.Id &&
                     e.CreatedAt >= now.AddHours(-24)))
                .Count();

            if (otpsCount >= 50)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message =
                    [
                        "You have exceeded the maximum number " +
                "of OTP requests today."
                    ]
                });
            }

            // إلغاء الأكواد السابقة عند طلب كود جديد.
            var previousOtps = await _applicationUserOTPRepository.GetAsync(
                e => e.ApplicationUserId == user.Id && !e.IsUsed);

            foreach (var previousOtp in previousOtps)
            {
                previousOtp.IsUsed = true;
                _applicationUserOTPRepository.Update(previousOtp);
            }

            var otp = RandomNumberGenerator
                .GetInt32(1000, 10000)
                .ToString();

            await _applicationUserOTPRepository.CreateAsync(
                new ApplicationUserOTP
                {
                    ApplicationUserId = user.Id,
                    OTP = otp,
                    IsUsed = false,
                    CreatedAt = now,
                    ExpiredAt = now.AddMinutes(10)
                });

            var saved = await _applicationUserOTPRepository.CommitAsync();

            if (saved <= 0)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = ["Failed to save OTP. Please try again."]
                    });
            }

            await _accountService.sendEmailAsync(
                EmailType.ForgetPassword,
                user,
                $"Your OTP is: {otp}. " +
                "It expires in 10 minutes. Please do not share it.");

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["OTP has been sent successfully."],
                Data = new
                {
                    ApplicationUserId = user.Id
                }
            });
        }



        [HttpPost("Validate-OTP")]
        public async Task<IActionResult> ValidateOTP(
            ValidateOTPRequest validateOTPRequest)
        {
            var user = await _userManager.FindByIdAsync(
                validateOTPRequest.ApplicationUserId);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["User not found."]
                });
            }

            // جلب أكواد هذا المستخدم فقط دون الاعتماد على navigation property.
            var userOtps = await _applicationUserOTPRepository.GetAsync(
                e => e.ApplicationUserId == user.Id);

            // التحقق من آخر كود صدر، وعدم الرجوع لكود أقدم.
            var otp = userOtps
                .OrderByDescending(e => e.Id)
                .FirstOrDefault();

            if (otp is null ||
                otp.IsUsed ||
                otp.ExpiredAt <= DateTime.UtcNow ||
                otp.FailedAttempts >= 5)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ["Invalid or expired OTP."]
                });
            }

            if (otp.OTP != validateOTPRequest.OTP)
            {
                otp.FailedAttempts++;
                if (otp.FailedAttempts >= 5)
                {
                    otp.IsUsed = true;
                }
                _applicationUserOTPRepository.Update(otp);
                if (await _applicationUserOTPRepository.CommitAsync() <= 0)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new APIResponce
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = ["Failed to verify OTP. Please try again later."]
                    });
                }
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = otp.IsUsed
                        ? ["Maximum OTP verification attempts reached. Please request a new OTP."]
                        : ["Invalid or expired OTP."]
                });
            }

            var resetToken =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            otp.IsUsed = true;
            _applicationUserOTPRepository.Update(otp);

            var saved = await _applicationUserOTPRepository.CommitAsync();

            if (saved <= 0)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = ["Failed to verify OTP. Please try again."]
                    });
            }

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["OTP verified successfully."],
                Data = new
                {
                    ApplicationUserId = user.Id,
                    ResetToken = resetToken
                }
            });
        }


        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(
     ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByIdAsync(
                resetPasswordRequest.ApplicationUserId);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["User not found."]
                });
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                resetPasswordRequest.ResetToken,
                resetPasswordRequest.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = result.Errors
                        .Select(error => error.Description)
                        .ToArray()
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
