using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.NET_Core_Web_API.Migrations
{
    /// <inheritdoc />
    public partial class EnumsRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new ApprovalStatus column to StudyGroups
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "StudyGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            // Migrate existing data from IsApproved to ApprovalStatus
            migrationBuilder.Sql(
                @"UPDATE StudyGroups SET ApprovalStatus = CASE WHEN IsApproved = 1 THEN 'Approved' ELSE 'Pending' END");

            // Drop the old IsApproved column
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "StudyGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the IsApproved column
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "StudyGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Restore IsApproved from ApprovalStatus
            migrationBuilder.Sql(
                @"UPDATE StudyGroups SET IsApproved = CASE WHEN ApprovalStatus = 'Approved' THEN 1 ELSE 0 END");

            // Drop the ApprovalStatus column
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "StudyGroups");
        }
    }
}
