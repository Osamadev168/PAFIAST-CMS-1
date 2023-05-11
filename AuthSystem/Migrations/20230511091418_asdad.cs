using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthSystem.Migrations
{
    /// <inheritdoc />
    public partial class asdad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestsDetail_Tests_Id",
                table: "TestsDetail");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TestsDetail",
                newName: "TestId");

            migrationBuilder.RenameIndex(
                name: "IX_TestsDetail_Id",
                table: "TestsDetail",
                newName: "IX_TestsDetail_TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestsDetail_Tests_TestId",
                table: "TestsDetail",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestsDetail_Tests_TestId",
                table: "TestsDetail");

            migrationBuilder.RenameColumn(
                name: "TestId",
                table: "TestsDetail",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_TestsDetail_TestId",
                table: "TestsDetail",
                newName: "IX_TestsDetail_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestsDetail_Tests_Id",
                table: "TestsDetail",
                column: "Id",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
