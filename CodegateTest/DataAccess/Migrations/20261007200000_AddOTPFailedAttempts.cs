using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodegateTest.Migrations
{
    public partial class AddOTPFailedAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "ApplicationUserOTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FailedAttempts", table: "ApplicationUserOTPs");
        }
    }
}
