using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlace.Api.Migrations
{
    /// <inheritdoc />
    public partial class CartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductVariants_ProductVariantId1",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductVariantId1",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId1",
                table: "CartItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId1",
                table: "CartItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductVariantId1",
                table: "CartItems",
                column: "ProductVariantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductVariants_ProductVariantId1",
                table: "CartItems",
                column: "ProductVariantId1",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }
    }
}
