using CodegateTest.Repositories.IRepositories;
using CodegateTest.Services;
using CodegateTest.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly IOtpService _otpService;
        private readonly IJWTHandler _jWTHandler;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManger,
            IAccountService accountService,
            IOtpService otpService,
            IJWTHandler jWTHandler,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository,
            ILogger<AccountsController> logger)
        {
            _userManager = userManager;
            _signInManger = signInManger;
            _accountService = accountService;
            _otpService = otpService;
            _jWTHandler = jWTHandler;
            _applicationUserOTPRepository = applicationUserOTPRepository;
            _logger = logger;
        }


        // ============================================================
        // Register
        // ============================================================

        [HttpPost("Register")]
        public async Task<IActionResult> Register(
            RegisterRequest registerRequest)
        {
            var user = registerRequest.Adapt<ApplicationUser>();

            var seed = Uri.EscapeDataString(
                $"{user.Fname}-{user.Lname}");

            user.ProfileImageUrl =
                $"https://api.dicebear.com/10.x/initials/svg?seed={seed}";

            var result = await _userManager.CreateAsync(
                user,
                registerRequest.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                SD.STUDENT_ROLE);

            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            var token =
                await _userManager.GenerateEmailConfirmationTokenAsync(user);

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
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        Message =
                        [
                            "Failed to generate confirmation link"
                        ]
                    });
            }

            await _accountService.sendEmailAsync(
                EmailType.ConfirmEmail,
                user,
                $"Click here to confirm your email: {confirmLink}");

            return StatusCode(
                StatusCodes.Status201Created,
                new APIResponce
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message =
                    [
                        "Your registration completed successfully."
                    ]
                });
        }


        // ============================================================
        // Login
        // ============================================================

        [HttpPost("Login")]
        public async Task<IActionResult> Login(
            LoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(
                loginRequest.UserName);

            if (user is null)
            {
                return Unauthorized(new APIResponce
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message =
                    [
                        "Invalid username or password."
                    ]
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
                    Message =
                    [
                        "Please confirm your email first."
                    ]
                });
            }

            if (result.IsLockedOut)
            {
                return Unauthorized(new APIResponce
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message =
                    [
                        "Your account has been locked due to multiple failed login attempts."
                    ]
                });
            }

            if (!result.Succeeded)
            {
                return Unauthorized(new APIResponce
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message =
                    [
                        "Invalid username or password."
                    ]
                });
            }

            var token =
                await _jWTHandler.GenerateTokenAsync(
                    user.Id,
                    user.Email!);

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message =
                [
                    $"Welcome, {user.UserName}"
                ],
                Data = token
            });
        }



        // Confirm Email


        [HttpGet("Confirm")]
        public async Task<IActionResult> Confirm(
            string userId,
            string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message =
                    [
                        "User not found."
                    ]
                });
            }

            var result =
                await _userManager.ConfirmEmailAsync(
                    user,
                    token);

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message =
                [
                    "Your email has been confirmed successfully."
                ]
            });
        }



        // Resend Email Confirmation


        [HttpGet("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(
            string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message =
                    [
                        "User not found."
                    ]
                });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message =
                    [
                        "Email is already confirmed."
                    ]
                });
            }

            var token =
                await _userManager.GenerateEmailConfirmationTokenAsync(user);

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
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        Message =
                        [
                            "Failed to generate confirmation link."
                        ]
                    });
            }

            await _accountService.sendEmailAsync(
                EmailType.ConfirmEmail,
                user,
                $"Click here to confirm your email: {confirmLink}");

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message =
                [
                    "Confirmation email has been sent successfully."
                ]
            });
        }




        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgetPassword(
            ForgetPasswordRequest request)
        {
            var user =
                await _userManager.FindByEmailAsync(request.Email);


            if (user is null)
            {
                return Ok(new APIResponce
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message =
                    [
                        "If the email exists, an OTP has been sent."
                    ]
                });
            }

            var now = DateTime.UtcNow;


            var recentOtps =
                await _applicationUserOTPRepository.GetAsync(
                    e =>
                        e.ApplicationUserId == user.Id &&
                        e.CreatedAt >= now.AddHours(-24));

            if (recentOtps.Count() >= 10)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message =
                    [
                        "You have exceeded the maximum number of OTP requests. Please try again later."
                    ]
                });
            }



            var previousOtps =
                await _applicationUserOTPRepository.GetAsync(
                    e =>
                        e.ApplicationUserId == user.Id &&
                        !e.IsUsed);

            foreach (var previousOtp in previousOtps)
            {
                previousOtp.IsUsed = true;

                _applicationUserOTPRepository.Update(previousOtp);
            }


            var otp = _otpService.GenerateOtp();

            var otpHash = _otpService.HashOtp(otp);


            var applicationUserOtp = new ApplicationUserOTP
            {
                ApplicationUserId = user.Id,

                OTPHash = otpHash,

                IsUsed = false,

                FailedAttempts = 0,

                CreatedAt = now,

                ExpiredAt = now.AddMinutes(10)
            };

            await _applicationUserOTPRepository.CreateAsync(
                applicationUserOtp);




            var saved =
                await _applicationUserOTPRepository.CommitAsync();

            if (saved <= 0)
            {
                _logger.LogError(
                    "Failed to save OTP for user {UserId}",
                    user.Id);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        StatusCode =
                            StatusCodes.Status500InternalServerError,

                        Message =
                        [
                            "Failed to process your request. Please try again later."
                        ]
                    });
            }




            try
            {
                await _accountService.sendEmailAsync(
                    EmailType.ForgetPassword,
                    user,
                    $"Your OTP is: {otp}. It expires in 10 minutes. Please do not share it with anyone.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send OTP email to user {UserId}",
                    user.Id);

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new APIResponce
                    {
                        StatusCode =
                            StatusCodes.Status503ServiceUnavailable,

                        Message =
                        [
                            "Unable to send the OTP. Please try again later."
                        ]
                    });
            }


            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,

                Message =
                [
                    "If the email exists, an OTP has been sent."
                ]
            });
        }



        [HttpPost("Validate-OTP")]
        public async Task<IActionResult> ValidateOTP(
            ValidateOTPRequest request)
        {
            var user =
                await _userManager.FindByIdAsync(
                    request.ApplicationUserId);

            if (user is null)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message =
                    [
                        "Invalid OTP."
                    ]
                });
            }

            var now = DateTime.UtcNow;



            var userOtps =
                await _applicationUserOTPRepository.GetAsync(
                    e =>
                        e.ApplicationUserId == user.Id &&
                        !e.IsUsed &&
                        e.ExpiredAt > now);


            // Get latest OTP
            var otp =
                userOtps
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefault();


  

            if (otp is null)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message =
                    [
                        "Invalid or expired OTP."
                    ]
                });
            }



            if (otp.FailedAttempts >= 5)
            {
                otp.IsUsed = true;

                _applicationUserOTPRepository.Update(otp);

                await _applicationUserOTPRepository.CommitAsync();

                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                    Message =
                    [
                        "Maximum OTP verification attempts reached. Please request a new OTP."
                    ]
                });
            }


            var isValid =
                _otpService.VerifyOtp(
                    request.OTP,
                    otp.OTPHash);


            if (!isValid)
            {
                otp.FailedAttempts++;

                if (otp.FailedAttempts >= 5)
                {
                    otp.IsUsed = true;
                }

                _applicationUserOTPRepository.Update(otp);

                await _applicationUserOTPRepository.CommitAsync();

                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                    Message = otp.IsUsed
                        ?
                        [
                            "Maximum OTP verification attempts reached. Please request a new OTP."
                        ]
                        :
                        [
                            "Invalid OTP."
                        ]
                });
            }



            otp.IsUsed = true;

            _applicationUserOTPRepository.Update(otp);


            // Generate Identity Password Reset Token
 

            var resetToken =
                await _userManager
                    .GeneratePasswordResetTokenAsync(user);

            // Save OTP state


            var saved =
                await _applicationUserOTPRepository.CommitAsync();

            if (saved <= 0)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new APIResponce
                    {
                        StatusCode =
                            StatusCodes.Status500InternalServerError,

                        Message =
                        [
                            "Failed to verify OTP. Please try again."
                        ]
                    });
            }



            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,

                Message =
                [
                    "OTP verified successfully."
                ],

                Data = new
                {
                    ApplicationUserId = user.Id,

                    ResetToken = resetToken
                }
            });
        }


    
        // Reset Password

        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordRequest request)
        {
            var user =
                await _userManager.FindByIdAsync(
                    request.ApplicationUserId);

            if (user is null)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                    Message =
                    [
                        "Invalid reset request."
                    ]
                });
            }

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    request.ResetToken,
                    request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                    Message =
                        result.Errors
                            .Select(e => e.Description)
                            .ToArray()
                });
            }

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,

                Message =
                [
                    "Password changed successfully."
                ]
            });
        }
    }
}