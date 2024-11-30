using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaWMVC.Data.IdentityMigrations
{
    /// <inheritdoc />
    public partial class Delete_Column_ResetPasswordOtp_And_ResetPasswordOtpExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetPasswordOtp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOtpExpiry",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordOtp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
