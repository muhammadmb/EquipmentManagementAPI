using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPropertiesinRentalContractEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledDate",
                table: "RentalContracts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinishedDate",
                table: "RentalContracts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RentalContracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SuspendedDate",
                table: "RentalContracts",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "RentalContracts");

            migrationBuilder.DropColumn(
                name: "FinishedDate",
                table: "RentalContracts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RentalContracts");

            migrationBuilder.DropColumn(
                name: "SuspendedDate",
                table: "RentalContracts");
        }
    }
}
