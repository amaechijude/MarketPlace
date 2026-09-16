using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlace.Api.Migrations
{
    /// <inheritdoc />
    public partial class Vendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vendors_Name",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "IsActivated",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Vendors");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedAt",
                table: "Vendors",
                newName: "SuspendedAt");

            migrationBuilder.RenameColumn(
                name: "DateJoined",
                table: "Vendors",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "Vendors",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Vendors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Vendors",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessAddress",
                table: "Vendors",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "Vendors",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNumber",
                table: "Vendors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Vendors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Vendors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Vendors",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Vendors",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreSlug",
                table: "Vendors",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupportEmail",
                table: "Vendors",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupportPhone",
                table: "Vendors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Vendors",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Vendors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Vendors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_ApprovalStatus",
                table: "Vendors",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_BusinessName",
                table: "Vendors",
                column: "BusinessName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_CreatedAt",
                table: "Vendors",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_IsActive",
                table: "Vendors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_StoreSlug",
                table: "Vendors",
                column: "StoreSlug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vendors_ApprovalStatus",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_BusinessName",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_CreatedAt",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_IsActive",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_StoreSlug",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BusinessAddress",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationNumber",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "StoreSlug",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "SupportEmail",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "SupportPhone",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Vendors");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Vendors",
                newName: "DateJoined");

            migrationBuilder.RenameColumn(
                name: "SuspendedAt",
                table: "Vendors",
                newName: "LastUpdatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "Vendors",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "Vendors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActivated",
                table: "Vendors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Vendors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Vendors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Vendors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Name",
                table: "Vendors",
                column: "Name",
                unique: true);
        }
    }
}
