using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodegateTest.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUserOTP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OTP",
                table: "ApplicationUserOTPs",
                newName: "OTPHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OTPHash",
                table: "ApplicationUserOTPs",
                newName: "OTP");
        }
    }
}
