using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaWMVC.Data.IdentityMigrations
{
    /// <inheritdoc />
    public partial class Add_Column_ResetPasswordOtp_And_ResetPasswordOtpExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Follow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolloweeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RaWMVCUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RaWMVCUserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Follow_AspNetUsers_RaWMVCUserId",
                        column: x => x.RaWMVCUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Follow_AspNetUsers_RaWMVCUserId1",
                        column: x => x.RaWMVCUserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Follow_RaWMVCUserId",
                table: "Follow",
                column: "RaWMVCUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Follow_RaWMVCUserId1",
                table: "Follow",
                column: "RaWMVCUserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Follow");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOtp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOtpExpiry",
                table: "AspNetUsers");
        }
    }
}
