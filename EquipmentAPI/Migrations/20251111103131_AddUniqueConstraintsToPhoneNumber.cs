using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "SupplierPhoneNumbers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CustomerPhoneNumbers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPhoneNumbers_Number",
                table: "SupplierPhoneNumbers",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPhoneNumbers_Number",
                table: "CustomerPhoneNumbers",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierPhoneNumbers_Number",
                table: "SupplierPhoneNumbers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerPhoneNumbers_Number",
                table: "CustomerPhoneNumbers");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "SupplierPhoneNumbers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CustomerPhoneNumbers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
