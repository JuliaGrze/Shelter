using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdoptionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdoptionStatus",
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
                    table.PrimaryKey("PK_AdoptionStatus", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AdoptionStatus",
                columns: new[] { "Id", "Code", "Description", "IsFinal", "Name" },
                values: new object[,]
                {
                    { 1, "Submitted", null, false, "Złożony wniosek" },
                    { 2, "InReview", null, false, "W trakcie weryfikacji" },
                    { 3, "HomeVisitScheduled", null, false, "Wizyta domowa umówiona" },
                    { 4, "HomeVisitCompleted", null, false, "Wizyta domowa odbyta" },
                    { 5, "Approved", null, false, "Zatwierdzony" },
                    { 6, "Rejected", null, true, "Odrzucony" },
                    { 7, "Withdrawn", null, true, "Wycofany przez wnioskodawcę" },
                    { 8, "ContractSigned", null, true, "Umowa podpisana" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionStatus_Code",
                table: "AdoptionStatus",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdoptionStatus");
        }
    }
}
