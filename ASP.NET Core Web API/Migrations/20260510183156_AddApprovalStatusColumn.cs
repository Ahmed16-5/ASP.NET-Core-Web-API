using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.NET_Core_Web_API.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
    name: "ApprovalStatus",
    table: "StudyGroups",
    type: "nvarchar(50)",
    maxLength: 50,
    nullable: false,
    defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
    name: "ApprovalStatus",
    table: "StudyGroups");
        }
    }
}
