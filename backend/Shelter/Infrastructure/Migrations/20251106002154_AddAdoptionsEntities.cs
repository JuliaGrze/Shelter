using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdoptionsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdoptionApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AdoptionStatusId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdoptionApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdoptionApplications_AdoptionStatus_AdoptionStatusId",
                        column: x => x.AdoptionStatusId,
                        principalTable: "AdoptionStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdoptionApplications_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdoptionApplications_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeVisitResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeVisitResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdoptionContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdoptionApplicationId = table.Column<int>(type: "int", nullable: false),
                    PdfUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PdfHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    VerificationQrContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdoptionContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdoptionContracts_AdoptionApplications_AdoptionApplicationId",
                        column: x => x.AdoptionApplicationId,
                        principalTable: "AdoptionApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdoptionContracts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HomeVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdoptionApplicationId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeVisitResultId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeVisits_AdoptionApplications_AdoptionApplicationId",
                        column: x => x.AdoptionApplicationId,
                        principalTable: "AdoptionApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeVisits_HomeVisitResults_HomeVisitResultId",
                        column: x => x.HomeVisitResultId,
                        principalTable: "HomeVisitResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "HomeVisitResults",
                columns: new[] { "Id", "Code", "Description", "IsFinal", "Name" },
                values: new object[,]
                {
                    { 1, "Pending", null, false, "W trakcie / oczekuje" },
                    { 2, "Passed", null, true, "Pozytywny" },
                    { 3, "Failed", null, true, "Negatywny" },
                    { 4, "Cancelled", null, true, "Odwołana" },
                    { 5, "Rescheduled", null, false, "Przełożona" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplications_AdoptionStatusId",
                table: "AdoptionApplications",
                column: "AdoptionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplications_AnimalId",
                table: "AdoptionApplications",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplications_ApplicationUserId",
                table: "AdoptionApplications",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplications_CreatedAt",
                table: "AdoptionApplications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionContracts_AdoptionApplicationId",
                table: "AdoptionContracts",
                column: "AdoptionApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionContracts_ApplicationUserId",
                table: "AdoptionContracts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionContracts_PdfHash",
                table: "AdoptionContracts",
                column: "PdfHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeVisitResults_Code",
                table: "HomeVisitResults",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeVisits_AdoptionApplicationId",
                table: "HomeVisits",
                column: "AdoptionApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeVisits_HomeVisitResultId",
                table: "HomeVisits",
                column: "HomeVisitResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdoptionContracts");

            migrationBuilder.DropTable(
                name: "HomeVisits");

            migrationBuilder.DropTable(
                name: "AdoptionApplications");

            migrationBuilder.DropTable(
                name: "HomeVisitResults");
        }
    }
}
