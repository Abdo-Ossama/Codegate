using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CodegateTest.Services
{
    public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public PasswordResetTokenProviderOptions()
        {
            Name = "CodegatePasswordReset";
            TokenLifespan = TimeSpan.FromMinutes(10);
        }
    }

    public class PasswordResetTokenProvider : DataProtectorTokenProvider<ApplicationUser>
    {
        public PasswordResetTokenProvider(
            IDataProtectionProvider protectionProvider,
            IOptions<PasswordResetTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<ApplicationUser>> logger)
            : base(protectionProvider,
                Microsoft.Extensions.Options.Options.Create<DataProtectionTokenProviderOptions>(options.Value), logger)
        {
        }
    }
}
