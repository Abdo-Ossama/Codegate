
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
            _applicationUserOTPRepository=applicationUserOTPRepository;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var user = registerRequest.Adapt<ApplicationUser>();
            var result = await _userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var ConfirmLink = Url.Action("Confirm", "Accounts", new { area = SD.IDENTITY_AREA, token, userId = user.Id }, Request.Scheme);
            if (ConfirmLink is null)
            {
                return StatusCode(500, new APIResponce
                {
                    Message = ["Failed to generate confirmation link"]
                });
            }

            await _accountService.sendEmailAsync(EmailType.ConfirmEmail, user, $"Click Here for Confirm Email {ConfirmLink}");
            var roleResult = await _userManager.AddToRoleAsync(
                 user,
                SD.STUDENT_ROLE
                );

            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }


            return Ok(new APIResponce
            {
                StatusCode = 201, // Create
                Message = ["Your Register is Completed Successfully .."],

            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.UserName);

            if (user is null)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["Invalid username/email or password"]
                });
            }

            var result = await _signInManger.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, false);

            if (result.IsNotAllowed)
            {
                return Unauthorized(new APIResponce { Message = ["Confirm email first"] });
            }

            if (result.IsLockedOut)
            {
                return Unauthorized(new APIResponce
                {
                    Message = ["Account is locked due to multiple failed attempts"]
                });
            }

            if (!result.Succeeded)
            {
                return Unauthorized(new APIResponce
                {
                    Message = ["Invalid username or password"]
                });
            }

            var roles = await _userManager.GetRolesAsync(user); // List of userRoles

            await _jWTHandler.GenerateTokenAsync(user.Id, loginRequest.Email);

            return Ok(new APIResponce()
            {
                StatusCode = 200,
                Message = [$"Welcome, {user.UserName}"]
            });

        }


        [HttpGet("Confirm")]
        public async Task<IActionResult> Confirm(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return BadRequest();
            await _userManager.ConfirmEmailAsync(user, token);
            return Ok(new APIResponce()
            {
                StatusCode = 200,
                Message = ["Your Email is Confirmed Successfully"]
            });
        }

        [HttpGet("ResendEmailConfiramtion")]
        public async Task<IActionResult> ResendEmailConfiramtion(string userId)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user is not null && !user.EmailConfirmed)
            {


                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var ConfirmLink = Url.Action("Confirm", "Accounts", new { area = SD.IDENTITY_AREA, token, userId = user.Id }, Request.Scheme);
                if (ConfirmLink is null)
                {
                    return StatusCode(500, new APIResponce
                    {
                        Message = ["Failed to generate confirmation link"]
                    });
                }

                await _accountService.sendEmailAsync(EmailType.ConfirmEmail, user, $"Click Here for Confirm Email {ConfirmLink}");
                var roleResult = await _userManager.AddToRoleAsync(
                     user,
                    SD.STUDENT_ROLE
                    );

                if (!roleResult.Succeeded)
                {
                    return BadRequest(roleResult.Errors);
                }

            }
            return Ok(new APIResponce()
            {
                StatusCode = 200,

                Message = ["Resend Email Confirmation Successfully"]
            }

                );


        }




        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordrequest)
        {

            var user = await _userManager.FindByEmailAsync(forgetPasswordrequest.Email);
         

            if (user is null) return NotFound();
            var otp = new Random().Next(1000, 9999).ToString();
            await _accountService.sendEmailAsync(EmailType.ForgetPassword, user!, $"This is the OTP : {otp} Please Don`t Share it ! ");
            var otpsCount = (await _applicationUserOTPRepository
                .GetAsync(e => e.ApplicationUserId == user.Id && e.CreatedAt >= DateTime.Now.AddHours(-24)))
                .Count();
            if (user is not null && otpsCount < 50)
            {
                await _applicationUserOTPRepository.CreateAsync(new ApplicationUserOTP
                {
                    ApplicationUserId = user.Id,
                    OTP = otp
                });
            }
            else if (otpsCount > 50)
            {

                return BadRequest(new APIResponce
                {
                    Message = ["Many Attemps Today , Please Try Again !"]
                });
            }

            await _applicationUserOTPRepository.CommitAsync();
            return Ok(new APIResponce
            {
               Message  = ["The OTP Sent to Email Successfully !",]
               
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
                    Message =[ "OTP verified successfully"],
                  
                });
            }

        }





        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {

            var user = await _userManager.FindByIdAsync(resetPasswordRequest.ApplicationUserId);
            if (user is null)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["User is not found "],
                });
                
            }



            var userToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, userToken, resetPasswordRequest.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce()
                {
                    Message =
        [
            "Password reset failed. Please make sure the reset link is valid and the new password meets the required requirements."
        ]
                });
            }

            return Ok(new APIResponce
            {
                Message = ["Change Password Successfully"],

            });

        }
    }
}
