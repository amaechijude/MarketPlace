using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlace.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorOrderAndWalletFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ProductVariants",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId",
                table: "Orders",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "DiscountAmountInKobo",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentStatus",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId",
                table: "Carts",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    DiscountType = table.Column<int>(type: "integer", nullable: false),
                    DiscountValue = table.Column<long>(type: "bigint", nullable: false),
                    MinOrderAmountInKobo = table.Column<long>(type: "bigint", nullable: true),
                    MaxDiscountAmountInKobo = table.Column<long>(type: "bigint", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    EndDate = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    AmountInKobo = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundRequests_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_RefundRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_RefundRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableBalanceInKobo = table.Column<long>(type: "bigint", nullable: false),
                    PendingBalanceInKobo = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PayoutRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountInKobo = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BankAccountName = table.Column<string>(type: "text", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: true),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ProcessedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutRequests_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountInKobo = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CouponId",
                table: "Orders",
                column: "CouponId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VendorId",
                table: "OrderItems",
                column: "VendorId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CouponId",
                table: "Carts",
                column: "CouponId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_VendorId",
                table: "Coupons",
                column: "VendorId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_WalletId",
                table: "PayoutRequests",
                column: "WalletId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_OrderId",
                table: "RefundRequests",
                column: "OrderId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_OrderItemId",
                table: "RefundRequests",
                column: "OrderItemId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_UserId",
                table: "RefundRequests",
                column: "UserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_VendorId",
                table: "Wallets",
                column: "VendorId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Coupons_CouponId",
                table: "Carts",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Vendors_VendorId",
                table: "OrderItems",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Carts_Coupons_CouponId", table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Vendors_VendorId",
                table: "OrderItems"
            );

            migrationBuilder.DropForeignKey(name: "FK_Orders_Coupons_CouponId", table: "Orders");

            migrationBuilder.DropTable(name: "Coupons");

            migrationBuilder.DropTable(name: "PayoutRequests");

            migrationBuilder.DropTable(name: "RefundRequests");

            migrationBuilder.DropTable(name: "WalletTransactions");

            migrationBuilder.DropTable(name: "Wallets");

            migrationBuilder.DropIndex(name: "IX_Orders_CouponId", table: "Orders");

            migrationBuilder.DropIndex(name: "IX_OrderItems_VendorId", table: "OrderItems");

            migrationBuilder.DropIndex(name: "IX_Carts_CouponId", table: "Carts");

            migrationBuilder.DropColumn(name: "xmin", table: "ProductVariants");

            migrationBuilder.DropColumn(name: "CouponId", table: "Orders");

            migrationBuilder.DropColumn(name: "DiscountAmountInKobo", table: "Orders");

            migrationBuilder.DropColumn(name: "FulfillmentStatus", table: "OrderItems");

            migrationBuilder.DropColumn(name: "VendorId", table: "OrderItems");

            migrationBuilder.DropColumn(name: "CouponId", table: "Carts");
        }
    }
}
