
using Microsoft.AspNetCore.Mvc;

namespace CodegateTest.Services.IServices
{
    public interface IAccountService
    {
         Task sendEmailAsync(EmailType emailType, ApplicationUser applicationUser, string msg);
    }
}
