using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.entities.Migrations.Dane
{
    public partial class InvoiceRemoveBruttoInWords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraInfo_IsInWords",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "ExtraInfo_TotalBruttoInWords",
                table: "InvoiceSell");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExtraInfo_IsInWords",
                table: "InvoiceSell",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExtraInfo_TotalBruttoInWords",
                table: "InvoiceSell",
                nullable: true);
        }
    }
}
