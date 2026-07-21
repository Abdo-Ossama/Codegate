using CodegateTest.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodegateTest.Services
{
    public enum EmailType
    {
        ConfirmEmail,
        ResendEmailConfirmation,
        ForgetPassword

    }
    public class AccountService :IAccountService
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(IEmailSender emailSender, UserManager<ApplicationUser> userManager)

        {
            _emailSender = emailSender;
            _userManager = userManager;
        }
        public async Task sendEmailAsync(EmailType emailType, ApplicationUser applicationUser , string msg)
        {
            if(emailType == EmailType.ConfirmEmail)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email! ,"Confirm Your Email", msg );
            }
           else if(emailType == EmailType.ResendEmailConfirmation)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email! ,"Resend Confirmation Email  ", msg );
            }
            else if(emailType == EmailType.ForgetPassword)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email! ," Forget Password ", msg );
            }
           
        }


    }
}
