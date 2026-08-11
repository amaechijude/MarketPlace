using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MarketPlace.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        private static readonly string[] columns = new[] { "Id", "CreateAt", "CreatedBy", "Name" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(25)",
                        maxLength: 25,
                        nullable: false
                    ),
                    CreateAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: false
                    ),
                    PasswordHash = table.Column<string>(
                        type: "character varying(2256)",
                        maxLength: 2256,
                        nullable: false
                    ),
                    NormalizedEmail = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: false
                    ),
                    EmailConfrimed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RolesId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.InsertData(
                table: "Roles",
                columns: columns,
                values: new object[,]
                {
                    {
                        new Guid("019d884e-2c65-721d-a025-4124c7208592"),
                        new DateTimeOffset(
                            new DateTime(2026, 4, 13, 19, 32, 6, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        "SuperAdmin",
                    },
                    {
                        new Guid("019d884f-78f1-7b14-8b0b-79116ef03b82"),
                        new DateTimeOffset(
                            new DateTime(2026, 4, 13, 19, 32, 6, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        "Admin",
                    },
                    {
                        new Guid("019d884f-c93c-7500-996a-5af40b07445a"),
                        new DateTimeOffset(
                            new DateTime(2026, 4, 13, 19, 32, 6, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        "Manager",
                    },
                    {
                        new Guid("019d8850-60a5-7066-85b2-bc78d43f19d8"),
                        new DateTimeOffset(
                            new DateTime(2026, 4, 13, 19, 32, 6, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        "Vendor",
                    },
                    {
                        new Guid("019dc959-d027-7104-be07-754b1966c487"),
                        new DateTimeOffset(
                            new DateTime(2026, 4, 26, 15, 26, 37, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        "ProductReviewModerator",
                    },
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UsersId",
                table: "UserRoles",
                column: "UsersId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserRoles");

            migrationBuilder.DropTable(name: "Roles");

            migrationBuilder.DropTable(name: "Users");
        }
    }
}
