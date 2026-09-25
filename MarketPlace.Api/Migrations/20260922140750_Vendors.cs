using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlace.Api.Migrations
{
    /// <inheritdoc />
    public partial class Vendors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Vendors_Users_UserId", table: "Vendors");

            migrationBuilder.DropIndex(name: "IX_Vendors_BusinessName", table: "Vendors");

            migrationBuilder.DropIndex(name: "IX_Vendors_Email", table: "Vendors");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Vendors",
                newName: "SuspendedBy"
            );

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Vendors",
                newName: "InternalEmail"
            );

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Vendors",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone"
            );

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Vendors",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageRating",
                table: "Vendors",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)",
                oldPrecision: 3,
                oldScale: 2,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Vendors",
                type: "text",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0
            );

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "Vendors",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_Users_UserId",
                table: "Vendors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Vendors_Users_UserId", table: "Vendors");

            migrationBuilder.DropColumn(name: "ApprovedBy", table: "Vendors");

            migrationBuilder.RenameColumn(
                name: "SuspendedBy",
                table: "Vendors",
                newName: "UpdatedBy"
            );

            migrationBuilder.RenameColumn(
                name: "InternalEmail",
                table: "Vendors",
                newName: "Email"
            );

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Vendors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(
                    new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    new TimeSpan(0, 0, 0, 0, 0)
                ),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Vendors",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageRating",
                table: "Vendors",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)",
                oldPrecision: 3,
                oldScale: 2
            );

            migrationBuilder.AlterColumn<int>(
                name: "ApprovalStatus",
                table: "Vendors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pending"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_BusinessName",
                table: "Vendors",
                column: "BusinessName",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Email",
                table: "Vendors",
                column: "Email",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_Users_UserId",
                table: "Vendors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
