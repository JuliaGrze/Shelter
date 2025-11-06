using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdoptionStatusData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AdoptionStatus",
                columns: new[] { "Id", "Code", "Description", "IsFinal", "Name" },
                values: new object[] { 9, "ContractGenerated", null, false, "Umowa wygenerowana" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AdoptionStatus",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
