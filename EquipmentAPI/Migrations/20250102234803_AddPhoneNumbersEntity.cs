using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumbersEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedDate",
                table: "PhoneNumbers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "PhoneNumbers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDate",
                table: "PhoneNumbers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "PhoneNumbers");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "PhoneNumbers");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "PhoneNumbers");
        }
    }
}
