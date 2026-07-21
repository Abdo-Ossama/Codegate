using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodegateTest.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseInstructorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseInstructor");

            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "Reviews",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IsApproved",
                table: "Reviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateTable(
                name: "CourseInstructor",
                columns: table => new
                {
                    CoursesId = table.Column<int>(type: "int", nullable: false),
                    InstructorsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseInstructor", x => new { x.CoursesId, x.InstructorsId });
                    table.ForeignKey(
                        name: "FK_CourseInstructor_Courses_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseInstructor_Instructors_InstructorsId",
                        column: x => x.InstructorsId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseInstructor_InstructorsId",
                table: "CourseInstructor",
                column: "InstructorsId");
        }
    }
}
