using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HB.OnlinePsikologMerkezi.Data.Migrations
{
    public partial class appointmentTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Start3DTime",
                table: "Appointment",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Start3DTime",
                table: "Appointment");
        }
    }
}
