using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGHSS.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeLogActivityUserNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogActivities_Users_UserId",
                table: "LogActivities");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "LogActivities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_LogActivities_Users_UserId",
                table: "LogActivities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogActivities_Users_UserId",
                table: "LogActivities");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "LogActivities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LogActivities_Users_UserId",
                table: "LogActivities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
