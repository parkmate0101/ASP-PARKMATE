using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkMate.Migrations
{
    /// <inheritdoc />
    public partial class InitialParkMateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pricing",
                columns: table => new
                {
                    PriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PricePerHour = table.Column<int>(type: "int", nullable: false),
                    PricePerDay = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing", x => x.PriceId);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "slot",
                columns: table => new
                {
                    SlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotNum = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SlotType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlotStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_slot", x => x.SlotId);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.UId);
                    table.ForeignKey(
                        name: "FK_users_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "login",
                columns: table => new
                {
                    LoginId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UId = table.Column<int>(type: "int", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoginType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login", x => x.LoginId);
                    table.ForeignKey(
                        name: "FK_login_users_UId",
                        column: x => x.UId,
                        principalTable: "users",
                        principalColumn: "UId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "otp_verification",
                columns: table => new
                {
                    OtpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UId = table.Column<int>(type: "int", nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    BlockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otp_verification", x => x.OtpId);
                    table.ForeignKey(
                        name: "FK_otp_verification_users_UId",
                        column: x => x.UId,
                        principalTable: "users",
                        principalColumn: "UId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle",
                columns: table => new
                {
                    VId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UId = table.Column<int>(type: "int", nullable: false),
                    VNum = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle", x => x.VId);
                    table.ForeignKey(
                        name: "FK_vehicle_users_UId",
                        column: x => x.UId,
                        principalTable: "users",
                        principalColumn: "UId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking",
                columns: table => new
                {
                    BId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UId = table.Column<int>(type: "int", nullable: false),
                    VId = table.Column<int>(type: "int", nullable: false),
                    VType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking", x => x.BId);
                    table.ForeignKey(
                        name: "FK_booking_slot_SlotId",
                        column: x => x.SlotId,
                        principalTable: "slot",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_users_UId",
                        column: x => x.UId,
                        principalTable: "users",
                        principalColumn: "UId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_vehicle_VId",
                        column: x => x.VId,
                        principalTable: "vehicle",
                        principalColumn: "VId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feedback",
                columns: table => new
                {
                    FId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback", x => x.FId);
                    table.ForeignKey(
                        name: "FK_feedback_booking_BId",
                        column: x => x.BId,
                        principalTable: "booking",
                        principalColumn: "BId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    PId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BId = table.Column<int>(type: "int", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.PId);
                    table.ForeignKey(
                        name: "FK_payment_booking_BId",
                        column: x => x.BId,
                        principalTable: "booking",
                        principalColumn: "BId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    RefundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BId = table.Column<int>(type: "int", nullable: false),
                    UId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.RefundId);
                    table.ForeignKey(
                        name: "FK_refunds_booking_BId",
                        column: x => x.BId,
                        principalTable: "booking",
                        principalColumn: "BId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_refunds_users_UId",
                        column: x => x.UId,
                        principalTable: "users",
                        principalColumn: "UId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "receipt",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PId = table.Column<int>(type: "int", nullable: false),
                    RecNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipt", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_receipt_payment_PId",
                        column: x => x.PId,
                        principalTable: "payment",
                        principalColumn: "PId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refund_receipt",
                columns: table => new
                {
                    RefundRecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefundId = table.Column<int>(type: "int", nullable: false),
                    RecNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_receipt", x => x.RefundRecId);
                    table.ForeignKey(
                        name: "FK_refund_receipt_refunds_RefundId",
                        column: x => x.RefundId,
                        principalTable: "refunds",
                        principalColumn: "RefundId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_SlotId",
                table: "booking",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_UId",
                table: "booking",
                column: "UId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_VId",
                table: "booking",
                column: "VId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_BId",
                table: "feedback",
                column: "BId");

            migrationBuilder.CreateIndex(
                name: "IX_login_UId",
                table: "login",
                column: "UId");

            migrationBuilder.CreateIndex(
                name: "IX_otp_verification_UId",
                table: "otp_verification",
                column: "UId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_BId",
                table: "payment",
                column: "BId");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_VType",
                table: "pricing",
                column: "VType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_receipt_PId",
                table: "receipt",
                column: "PId");

            migrationBuilder.CreateIndex(
                name: "IX_receipt_RecNo",
                table: "receipt",
                column: "RecNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refund_receipt_RecNo",
                table: "refund_receipt",
                column: "RecNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refund_receipt_RefundId",
                table: "refund_receipt",
                column: "RefundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refunds_BId",
                table: "refunds",
                column: "BId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refunds_UId",
                table: "refunds",
                column: "UId");

            migrationBuilder.CreateIndex(
                name: "IX_slot_SlotNum",
                table: "slot",
                column: "SlotNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Contact",
                table: "users",
                column: "Contact",
                unique: true,
                filter: "[Contact] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_UId",
                table: "vehicle",
                column: "UId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_VNum",
                table: "vehicle",
                column: "VNum",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedback");

            migrationBuilder.DropTable(
                name: "login");

            migrationBuilder.DropTable(
                name: "otp_verification");

            migrationBuilder.DropTable(
                name: "pricing");

            migrationBuilder.DropTable(
                name: "receipt");

            migrationBuilder.DropTable(
                name: "refund_receipt");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "booking");

            migrationBuilder.DropTable(
                name: "slot");

            migrationBuilder.DropTable(
                name: "vehicle");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "role");
        }
    }
}
